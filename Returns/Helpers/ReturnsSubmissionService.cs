using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class ReturnsSubmissionService : IReturnsSubmissionService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<ReturnsSubmissionService> _logger;
        private readonly FormProcessingService _formProcessor;
        private readonly IEmailService _emailService;
        private readonly IReturnAssignmentService _assignmentService;
        private readonly IWorkflowEngineService _workflowService;
        private readonly ICamelsAnalysisService _analysisService;
        private readonly IComplianceService _complianceService;

        public ReturnsSubmissionService(
            ReturnsDbContext context,
            ILogger<ReturnsSubmissionService> logger,
            FormProcessingService formProcessor,
            IEmailService emailService,
            IReturnAssignmentService assignmentService,
            IWorkflowEngineService workflowService,
            ICamelsAnalysisService analysisService,
            IComplianceService complianceService)
        {
            _context = context;
            _logger = logger;
            _formProcessor = formProcessor;
            _emailService = emailService;
            _assignmentService = assignmentService;
            _workflowService = workflowService;
            _analysisService = analysisService;
            _complianceService = complianceService;
        }

        public async Task<List<ExpectedReturnDto>> GetExpectedReturnsAsync(string saccoId, string? periodId = null)
        {
            try
            {
                // Get SACCO details to know when they were onboarded
                var saccoDetails = await _complianceService.GetSaccoByIdAsync(saccoId);
                if (saccoDetails == null)
                {
                    return new List<ExpectedReturnDto>();
                }

                // Build query for periods
                var periodsQuery = _context.ReturnPeriods
                    .Include(p => p.ReportingYear)
                    .Include(p => p.FrequencyCatalog)
                    .Where(p => p.IsActive);

                // If specific period requested
                if (!string.IsNullOrEmpty(periodId))
                {
                    periodsQuery = periodsQuery.Where(p => p.Id == periodId);
                }
                else
                {
                    // Get periods from SACCO onboarding date onwards
                    var onboardingDate = saccoDetails.CreatedAt ?? DateTime.Now.AddYears(-1);
                    periodsQuery = periodsQuery.Where(p => p.StartDate >= onboardingDate.Date);
                }

                var periods = await periodsQuery.ToListAsync();

                // Get all active forms for this SACCO type
                var forms = await _context.ReturnForms
                    .Where(f => f.IsActive && f.SaccoTypeId == saccoDetails.SaccoType)
                    .ToListAsync();

                // Get already filed returns
                var filedReturns = await _context.Returns
                    .Where(r => r.SaccoId == saccoId && r.IsActiveVersion)
                    .Select(r => new { r.PeriodId, r.Id, r.SubmittedAt })
                    .ToListAsync();

                var filedByPeriod = filedReturns
                    .GroupBy(r => r.PeriodId)
                    .ToDictionary(g => g.Key, g => g.First());

                // Get waived returns
                var waivedReturns = await _context.WaivedReturns
                    .Where(w => w.SaccoId == saccoId)
                    .ToListAsync();

                var waivedLookup = waivedReturns
                    .ToLookup(w => new { w.PeriodId, w.FormId });

                // Calculate expected returns
                var expectedReturns = new List<ExpectedReturnDto>();

                foreach (var period in periods)
                {
                    // Get forms applicable for this period's frequency
                    var applicableForms = forms
                        .Where(f => f.Frequency == period.FrequencyCatalog.Code || f.Frequency == "ALL")
                        .ToList();

                    foreach (var form in applicableForms)
                    {
                        var dueDate = period.GetDueDate();
                        var hasFiled = filedByPeriod.TryGetValue(period.Id, out var filedReturn);
                        var isWaived = waivedLookup.Contains(new { PeriodId = period.Id, FormId = form.Id });
                        var waiverInfo = isWaived ? waivedLookup[new { PeriodId = period.Id, FormId = form.Id }].First() : null;

                        // Calculate status
                        ExpectedStatus status;
                        if (isWaived)
                            status = ExpectedStatus.Waived;
                        else if (hasFiled)
                            status = ExpectedStatus.Filed;
                        else if (DateTime.Now > dueDate)
                            status = ExpectedStatus.Late;
                        else
                            status = ExpectedStatus.Due;

                        expectedReturns.Add(new ExpectedReturnDto
                        {
                            Id = $"{period.Id}_{form.Id}", // Composite ID
                            PeriodId = period.Id,
                            PeriodName = period.Name,
                            PeriodStartDate = period.StartDate,
                            PeriodEndDate = period.EndDate,
                            FormId = form.Id,
                            FormCode = form.Code,
                            FormName = form.FormName,
                            DueDate = dueDate,
                            Status = status,
                            DaysUntilDue = (dueDate - DateTime.Now).Days,
                            CanFile = !isWaived && !hasFiled,
                            IsWaived = isWaived,
                            WaivedReason = waiverInfo?.WaivedReason,
                            WaivedDate = waiverInfo?.WaivedDate,
                            ReturnId = filedReturn?.Id,
                            FiledDate = filedReturn?.SubmittedAt,
                            IsLate = hasFiled && filedReturn.SubmittedAt > dueDate
                        });
                    }
                }

                return expectedReturns
                    .OrderBy(e => e.DueDate)
                    .ThenBy(e => e.FormName)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expected returns for SACCO {SaccoId}", saccoId);
                throw;
            }
        }

        public async Task<List<ExpectedReturnDto>> GetDueReturnsAsync(string saccoId)
        {
            var allExpected = await GetExpectedReturnsAsync(saccoId);
            return allExpected
                .Where(e => e.Status == ExpectedStatus.Due || e.Status == ExpectedStatus.Late)
                .OrderBy(e => e.DueDate)
                .ToList();
        }

        public async Task<ReturnFilingEligibility> CheckFilingEligibilityAsync(
            string saccoId, 
            string periodId, 
            List<string> formIds)
        {
            try
            {
                // Get the period details
                var period = await _context.ReturnPeriods
                    .Include(p => p.FrequencyCatalog)
                    .FirstOrDefaultAsync(p => p.Id == periodId);

                if (period == null)
                {
                    return new ReturnFilingEligibility
                    {
                        CanFile = false,
                        ReasonIfCannot = "Invalid period specified"
                    };
                }

                var dueDate = period.GetDueDate();
                var eligibility = new ReturnFilingEligibility
                {
                    PeriodName = period.Name,
                    DueDate = dueDate
                };

                // Check if there's already an active return for this period
                var existingReturn = await _context.Returns
                    .FirstOrDefaultAsync(r => r.SaccoId == saccoId 
                        && r.PeriodId == periodId 
                        && r.IsActiveVersion);

                if (existingReturn != null)
                {
                    eligibility.CanFile = false;
                    eligibility.ReasonIfCannot = "Returns already filed for this period";
                    return eligibility;
                }

                // Get SACCO details
                var saccoDetails = await _complianceService.GetSaccoByIdAsync(saccoId);
                if (saccoDetails == null)
                {
                    eligibility.CanFile = false;
                    eligibility.ReasonIfCannot = "SACCO not found";
                    return eligibility;
                }

                // Get forms required for this period
                var requiredForms = await _context.ReturnForms
                    .Where(f => f.IsActive 
                        && f.SaccoTypeId == saccoDetails.SaccoType
                        && (f.Frequency == period.FrequencyCatalog.Code || f.Frequency == "ALL"))
                    .Select(f => f.Id)
                    .ToListAsync();

                // Check for waived forms
                var waivedForms = await _context.WaivedReturns
                    .Where(w => w.SaccoId == saccoId && w.PeriodId == periodId)
                    .Select(w => w.FormId)
                    .ToListAsync();

                // Remove waived forms from required
                var actuallyRequired = requiredForms.Except(waivedForms).ToList();

                eligibility.MissingForms = actuallyRequired
                    .Except(formIds)
                    .ToList();

                if (eligibility.MissingForms.Any())
                {
                    eligibility.CanFile = false;
                    eligibility.ReasonIfCannot = "Missing required forms";
                }
                else
                {
                    eligibility.CanFile = true;
                }

                return eligibility;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking filing eligibility");
                throw;
            }
        }

        public async Task<ReturnSubmissionResult> SubmitReturnsAsync(NewReturnSubmissionDto submission)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = new ReturnSubmissionResult();

                // Validate eligibility
                var formIds = submission.FormSubmissions.Select(f => f.FormId).ToList();
                var eligibility = await CheckFilingEligibilityAsync(
                    submission.SaccoId, 
                    submission.PeriodId, 
                    formIds);

                if (!eligibility.CanFile)
                {
                    result.Success = false;
                    result.Errors.Add(eligibility.ReasonIfCannot ?? "Cannot file returns");
                    return result;
                }

                // Get period details
                var period = await _context.ReturnPeriods
                    .Include(p => p.ReportingYear)
                    .FirstOrDefaultAsync(p => p.Id == submission.PeriodId);

                if (period == null)
                {
                    result.Success = false;
                    result.Errors.Add("Invalid period");
                    return result;
                }

                // Create the main return record
                var newReturn = new Return
                {
                    Id = Guid.NewGuid().ToString(),
                    PeriodId = submission.PeriodId,
                    SaccoId = submission.SaccoId,
                    SaccoType = submission.SaccoType,
                    SaccoName = submission.SaccoId, // This should come from SACCO service
                    SubmittedAt = DateTime.Now,
                    IsActiveVersion = true,
                    VersionNumber = 1,
                    
                    // Legacy fields - will be removed later
                    ReturnFor = period.EndDate,
                    Period = period.Name,
                    Year = period.ReportingYear.Year.ToString()
                };

                _context.Returns.Add(newReturn);
                await _context.SaveChangesAsync();

                // Process each form submission
                foreach (var formSubmission in submission.FormSubmissions)
                {
                    try
                    {
                        // Process the form based on type
                        // This is where you'd call your existing form processing logic
                        // For now, I'll add a placeholder
                        
                        result.ProcessedForms[formSubmission.FormId] = "Processed";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing form {FormId}", formSubmission.FormId);
                        result.Errors.Add($"Error processing form {formSubmission.FormId}: {ex.Message}");
                    }
                }

                // Run consistency checks
                // TODO: Implement consistency check logic here

                // Assign the return
                var assignmentResult = await _assignmentService.AssignReturnAsync(newReturn, submission.SaccoId);
                if (!assignmentResult.Success)
                {
                    result.Warnings.Add($"Could not assign return: {assignmentResult.ErrorMessage}");
                }

                // Start workflow
                if (submission.SaccoType == Constants.SaccoType.DepositTaking.ToString())
                {
                    var analysisResult = await _analysisService.CalculateAnalysisAsync(newReturn.Id, newReturn.SaccoType);
                    await _workflowService.StartWorkflowAsync(newReturn, analysisResult.OverallRating);
                }

                // Send confirmation email
                await _emailService.SendEmailAsync(
                    submission.SaccoId, // This should be email address
                    "Return Submission Confirmation",
                    $"Your returns for period {period.Name} have been successfully submitted.");

                await transaction.CommitAsync();

                result.Success = true;
                result.ReturnId = newReturn.Id;
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error submitting returns");
                throw;
            }
        }

        public async Task<List<SubmittedReturnSummaryDto>> GetSubmittedReturnsAsync(string saccoId, int? year = null)
        {
            try
            {
                var query = _context.Returns
                    .Include(r => r.Period)
                        .ThenInclude(p => p.ReportingYear)
                    .Include(r => r.Period)
                        .ThenInclude(p => p.FrequencyCatalog)
                    .Where(r => r.SaccoId == saccoId && r.IsActiveVersion);

                if (year.HasValue)
                {
                    query = query.Where(r => r.Period.ReportingYear.Year == year.Value);
                }

                var returns = await query
                    .OrderByDescending(r => r.SubmittedAt)
                    .ToListAsync();

                // Get SACCO details
                var saccoDetails = await _complianceService.GetSaccoByIdAsync(saccoId);

                // Map to DTOs
                var summaries = new List<SubmittedReturnSummaryDto>();
                foreach (var ret in returns)
                {
                    // Get expected forms for this period
                    var expectedForms = await _context.ReturnForms
                        .Where(f => f.IsActive 
                            && f.SaccoTypeId == saccoDetails.SaccoType
                            && (f.Frequency == ret.Period.FrequencyCatalog.Code || f.Frequency == "ALL"))
                        .CountAsync();

                    // Get waived forms count
                    var waivedCount = await _context.WaivedReturns
                        .Where(w => w.SaccoId == saccoId && w.PeriodId == ret.PeriodId)
                        .CountAsync();

                    var summary = new SubmittedReturnSummaryDto
                    {
                        ReturnId = ret.Id,
                        PeriodId = ret.PeriodId,
                        PeriodName = ret.Period.Name,
                        PeriodStartDate = ret.Period.StartDate,
                        PeriodEndDate = ret.Period.EndDate,
                        Year = ret.Period.ReportingYear.Year.ToString(),
                        SubmittedAt = ret.SubmittedAt,
                        SaccoId = ret.SaccoId,
                        SaccoName = ret.SaccoName,
                        SaccoType = ret.SaccoType,
                        IsConsistent = !ret.IsNotConsistent,
                        ConsistencyErrors = ret.ConsistentErrorMessage,
                        VersionNumber = ret.VersionNumber,
                        IsActiveVersion = ret.IsActiveVersion,
                        PreviousVersionId = ret.PreviousVersionId,
                        ApprovalStatus = "Pending", // Get from workflow
                        TotalFormsExpected = expectedForms - waivedCount
                    };

                    // Count submitted forms based on SACCO type
                    if (ret.SaccoType == Constants.SaccoType.DepositTaking.ToString())
                    {
                        summary.TotalFormsSubmitted = CountSubmittedFormsDT(ret);
                        summary.LateSubmissions = CountLateFormsDT(ret);
                    }
                    else
                    {
                        summary.TotalFormsSubmitted = CountSubmittedFormsNWDT(ret);
                        summary.LateSubmissions = CountLateFormsNWDT(ret);
                    }

                    summaries.Add(summary);
                }

                return summaries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submitted returns");
                throw;
            }
        }

        private int CountSubmittedFormsDT(Return ret)
        {
            int count = 0;
            if (ret.CapitalAdequencies.Any()) count++;
            if (ret.LiquidityReturns.Any()) count++;
            if (ret.DepositReturns.Any()) count++;
            if (ret.RiskClassifications.Any()) count++;
            if (ret.InvestmentReturns.Any()) count++;
            if (ret.StatementOfFinancialPositionReturns.Any()) count++;
            if (ret.StatementOfComprehensiveIncomeReturns.Any()) count++;
            if (ret.ManagementReturns.Any()) count++;
            if (ret.OtherReturns.Any()) count += ret.OtherReturns.Count;
            return count;
        }

        private int CountSubmittedFormsNWDT(Return ret)
        {
            int count = 0;
            if (ret.NDWTCapitalAdequacyReturns.Any()) count++;
            if (ret.NWDTLiquidityReturns.Any()) count++;
            if (ret.NWDTDepositReturns.Any()) count++;
            if (ret.NWDTRiskClassificationReturns.Any()) count++;
            if (ret.NWDTInvestmentReturns.Any()) count++;
            if (ret.NWDTFinancialPositionReturns.Any()) count++;
            if (ret.NWDTComprehensiveIncomeReturns.Any()) count++;
            if (ret.ManagementReturns.Any()) count++;
            if (ret.OtherReturns.Any()) count += ret.OtherReturns.Count;
            return count;
        }

        private int CountLateFormsDT(Return ret)
        {
            int count = 0;
            if (ret.CapitalAdequencies.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.LiquidityReturns.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.DepositReturns.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.RiskClassifications.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.InvestmentReturns.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.StatementOfFinancialPositionReturns.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.StatementOfComprehensiveIncomeReturns.Any(f => f.DaysLateBy > 0)) count++;
            return count;
        }

        private int CountLateFormsNWDT(Return ret)
        {
            int count = 0;
            if (ret.NDWTCapitalAdequacyReturns.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.NWDTLiquidityReturns.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.NWDTDepositReturns.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.NWDTRiskClassificationReturns.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.NWDTInvestmentReturns.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.NWDTFinancialPositionReturns.Any(f => f.DaysLateBy > 0)) count++;
            if (ret.NWDTComprehensiveIncomeReturns.Any(f => f.DaysLateBy > 0)) count++;
            return count;
        }

        public async Task<bool> WaiveReturnAsync(string expectedReturnId, string reason, string waivedBy)
        {
            try
            {
                // Parse the composite ID (periodId_formId)
                var parts = expectedReturnId.Split('_');
                if (parts.Length != 3) // Should be saccoId_periodId_formId
                    return false;

                var saccoId = parts[0];
                var periodId = parts[1];
                var formId = parts[2];

                // Check if already waived
                var existingWaiver = await _context.WaivedReturns
                    .FirstOrDefaultAsync(w => w.SaccoId == saccoId 
                        && w.PeriodId == periodId 
                        && w.FormId == formId);

                if (existingWaiver != null)
                    return false; // Already waived

                var waivedReturn = new WaivedReturn
                {
                    Id = Guid.NewGuid().ToString(),
                    SaccoId = saccoId,
                    PeriodId = periodId,
                    FormId = formId,
                    WaivedReason = reason,
                    WaivedDate = DateTime.Now,
                    WaivedBy = waivedBy
                };

                _context.WaivedReturns.Add(waivedReturn);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiving return");
                return false;
            }
        }

        public async Task<List<ExpectedReturnDto>> GetExpectedReturnsByMonthAsync(string saccoId, int year, int month)
        {
            try
            {
                // Validate input
                if (month < 1 || month > 12)
                {
                    throw new ArgumentException("Month must be between 1 and 12");
                }

                // Get SACCO details
                var saccoDetails = await _complianceService.GetSaccoByIdAsync(saccoId);
                if (saccoDetails == null)
                {
                    return new List<ExpectedReturnDto>();
                }

                var firstDayOfMonth = new DateTime(year, month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Get periods where due date falls within this month
                var periods = await _context.ReturnPeriods
                    .Include(p => p.ReportingYear)
                    .Include(p => p.FrequencyCatalog)
                    .Where(p => p.IsActive)
                    .ToListAsync();

                // Filter periods by due date in memory since GetDueDate() is a method
                var periodsInMonth = periods
                    .Where(p => {
                        var dueDate = p.GetDueDate();
                        return dueDate >= firstDayOfMonth && dueDate <= lastDayOfMonth;
                    })
                    .ToList();

                // Get applicable forms
                var forms = await _context.ReturnForms
                    .Where(f => f.IsActive && f.SaccoTypeId == saccoDetails.SaccoType)
                    .ToListAsync();

                // Get filed returns for these periods
                var periodIds = periodsInMonth.Select(p => p.Id).ToList();
                var filedReturns = await _context.Returns
                    .Where(r => r.SaccoId == saccoId 
                        && r.IsActiveVersion 
                        && periodIds.Contains(r.PeriodId))
                    .Select(r => new { r.PeriodId, r.Id, r.SubmittedAt })
                    .ToListAsync();

                var filedByPeriod = filedReturns
                    .GroupBy(r => r.PeriodId)
                    .ToDictionary(g => g.Key, g => g.First());

                // Get waived returns
                var waivedReturns = await _context.WaivedReturns
                    .Where(w => w.SaccoId == saccoId && periodIds.Contains(w.PeriodId))
                    .ToListAsync();

                var waivedLookup = waivedReturns
                    .ToLookup(w => new { w.PeriodId, w.FormId });

                // Build expected returns
                var expectedReturns = new List<ExpectedReturnDto>();

                foreach (var period in periodsInMonth)
                {
                    var applicableForms = forms
                        .Where(f => f.Frequency == period.FrequencyCatalog.Code || f.Frequency == "ALL")
                        .ToList();

                    foreach (var form in applicableForms)
                    {
                        var dueDate = period.GetDueDate();
                        var hasFiled = filedByPeriod.TryGetValue(period.Id, out var filedReturn);
                        var isWaived = waivedLookup.Contains(new { PeriodId = period.Id, FormId = form.Id });
                        var waiverInfo = isWaived ? waivedLookup[new { PeriodId = period.Id, FormId = form.Id }].First() : null;

                        // Calculate status
                        ExpectedStatus status;
                        if (isWaived)
                            status = ExpectedStatus.Waived;
                        else if (hasFiled)
                            status = ExpectedStatus.Filed;
                        else if (DateTime.Now > dueDate)
                            status = ExpectedStatus.Late;
                        else
                            status = ExpectedStatus.Due;

                        expectedReturns.Add(new ExpectedReturnDto
                        {
                            Id = $"{period.Id}_{form.Id}",
                            PeriodId = period.Id,
                            PeriodName = period.Name,
                            PeriodStartDate = period.StartDate,
                            PeriodEndDate = period.EndDate,
                            FormId = form.Id,
                            FormCode = form.Code,
                            FormName = form.FormName,
                            DueDate = dueDate,
                            Status = status,
                            DaysUntilDue = (dueDate - DateTime.Now).Days,
                            CanFile = !isWaived && !hasFiled,
                            IsWaived = isWaived,
                            WaivedReason = waiverInfo?.WaivedReason,
                            WaivedDate = waiverInfo?.WaivedDate,
                            ReturnId = filedReturn?.Id,
                            FiledDate = filedReturn?.SubmittedAt,
                            IsLate = hasFiled && filedReturn.SubmittedAt > dueDate
                        });
                    }
                }

                return expectedReturns
                    .OrderBy(e => e.DueDate)
                    .ThenBy(e => e.FormName)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expected returns by month");
                throw;
            }
        }

        public async Task<List<ExpectedReturnDto>> GetExpectedReturnsByYearAsync(string saccoId, int year)
        {
            try
            {
                // Get SACCO details
                var saccoDetails = await _complianceService.GetSaccoByIdAsync(saccoId);
                if (saccoDetails == null)
                {
                    return new List<ExpectedReturnDto>();
                }

                var firstDayOfYear = new DateTime(year, 1, 1);
                var lastDayOfYear = new DateTime(year, 12, 31);

                // Get periods where due date falls within this year
                var periods = await _context.ReturnPeriods
                    .Include(p => p.ReportingYear)
                    .Include(p => p.FrequencyCatalog)
                    .Where(p => p.IsActive && p.ReportingYear.Year == year)
                    .ToListAsync();

                // Further filter by due date and SACCO onboarding date
                var onboardingDate = saccoDetails.CreatedAt ?? DateTime.Now.AddYears(-1);
                var periodsInYear = periods
                    .Where(p => {
                        var dueDate = p.GetDueDate();
                        return dueDate >= firstDayOfYear 
                            && dueDate <= lastDayOfYear
                            && p.StartDate >= onboardingDate.Date;
                    })
                    .ToList();

                // The rest is similar to GetExpectedReturnsByMonthAsync
                var forms = await _context.ReturnForms
                    .Where(f => f.IsActive && f.SaccoTypeId == saccoDetails.SaccoType)
                    .ToListAsync();

                var periodIds = periodsInYear.Select(p => p.Id).ToList();
                var filedReturns = await _context.Returns
                    .Where(r => r.SaccoId == saccoId 
                        && r.IsActiveVersion 
                        && periodIds.Contains(r.PeriodId))
                    .Select(r => new { r.PeriodId, r.Id, r.SubmittedAt })
                    .ToListAsync();

                var filedByPeriod = filedReturns
                    .GroupBy(r => r.PeriodId)
                    .ToDictionary(g => g.Key, g => g.First());

                var waivedReturns = await _context.WaivedReturns
                    .Where(w => w.SaccoId == saccoId && periodIds.Contains(w.PeriodId))
                    .ToListAsync();

                var waivedLookup = waivedReturns
                    .ToLookup(w => new { w.PeriodId, w.FormId });

                var expectedReturns = new List<ExpectedReturnDto>();

                foreach (var period in periodsInYear)
                {
                    var applicableForms = forms
                        .Where(f => f.Frequency == period.FrequencyCatalog.Code || f.Frequency == "ALL")
                        .ToList();

                    foreach (var form in applicableForms)
                    {
                        var dueDate = period.GetDueDate();
                        var hasFiled = filedByPeriod.TryGetValue(period.Id, out var filedReturn);
                        var isWaived = waivedLookup.Contains(new { PeriodId = period.Id, FormId = form.Id });
                        var waiverInfo = isWaived ? waivedLookup[new { PeriodId = period.Id, FormId = form.Id }].First() : null;

                        ExpectedStatus status;
                        if (isWaived)
                            status = ExpectedStatus.Waived;
                        else if (hasFiled)
                            status = ExpectedStatus.Filed;
                        else if (DateTime.Now > dueDate)
                            status = ExpectedStatus.Late;
                        else
                            status = ExpectedStatus.Due;

                        expectedReturns.Add(new ExpectedReturnDto
                        {
                            Id = $"{period.Id}_{form.Id}",
                            PeriodId = period.Id,
                            PeriodName = period.Name,
                            PeriodStartDate = period.StartDate,
                            PeriodEndDate = period.EndDate,
                            FormId = form.Id,
                            FormCode = form.Code,
                            FormName = form.FormName,
                            DueDate = dueDate,
                            Status = status,
                            DaysUntilDue = (dueDate - DateTime.Now).Days,
                            CanFile = !isWaived && !hasFiled,
                            IsWaived = isWaived,
                            WaivedReason = waiverInfo?.WaivedReason,
                            WaivedDate = waiverInfo?.WaivedDate,
                            ReturnId = filedReturn?.Id,
                            FiledDate = filedReturn?.SubmittedAt,
                            IsLate = hasFiled && filedReturn.SubmittedAt > dueDate
                        });
                    }
                }

                return expectedReturns
                    .OrderBy(e => e.DueDate)
                    .ThenBy(e => e.FormName)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expected returns by year");
                throw;
            }
        }
    }
}