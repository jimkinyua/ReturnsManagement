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

        public ReturnsSubmissionService(
            ReturnsDbContext context,
            ILogger<ReturnsSubmissionService> logger,
            FormProcessingService formProcessor,
            IEmailService emailService,
            IReturnAssignmentService assignmentService,
            IWorkflowEngineService workflowService,
            ICamelsAnalysisService analysisService)
        {
            _context = context;
            _logger = logger;
            _formProcessor = formProcessor;
            _emailService = emailService;
            _assignmentService = assignmentService;
            _workflowService = workflowService;
            _analysisService = analysisService;
        }

        public async Task<List<ExpectedReturnDto>> GetExpectedReturnsAsync(string saccoId, string? periodId = null)
        {
            try
            {
                var query = _context.ExpectedReturns
                    .Include(e => e.Period)
                        .ThenInclude(p => p.ReportingYear)
                    .Include(e => e.Period)
                        .ThenInclude(p => p.FrequencyCatalog)
                    .Include(e => e.Form)
                    .Where(e => e.SaccoId == saccoId);

                if (!string.IsNullOrEmpty(periodId))
                {
                    query = query.Where(e => e.PeriodId == periodId);
                }

                var expectedReturns = await query.ToListAsync();

                // Get all returns for this SACCO to check which are filed
                var filedReturns = await _context.Returns
                    .Where(r => r.SaccoId == saccoId && r.IsActiveVersion)
                    .Select(r => new { r.PeriodId, r.Id, r.SubmittedAt })
                    .ToListAsync();

                var filedReturnsByPeriod = filedReturns
                    .GroupBy(r => r.PeriodId)
                    .ToDictionary(g => g.Key, g => g.First());

                // Map to DTOs with calculated status
                var dtos = expectedReturns.Select(e =>
                {
                    var hasFiled = filedReturnsByPeriod.TryGetValue(e.PeriodId, out var filedReturn);
                    e.HasAssociatedReturn = hasFiled;

                    return new ExpectedReturnDto
                    {
                        Id = e.Id,
                        PeriodId = e.PeriodId,
                        PeriodName = e.Period.Name,
                        PeriodStartDate = e.Period.StartDate,
                        PeriodEndDate = e.Period.EndDate,
                        FormId = e.FormId,
                        FormCode = e.Form.Code,
                        FormName = e.Form.FormName,
                        DueDate = e.DueDate,
                        Status = e.Status,
                        DaysUntilDue = e.DaysUntilDue(),
                        CanFile = e.CanFileReturn(),
                        IsWaived = e.IsWaived,
                        WaivedReason = e.WaivedReason,
                        WaivedDate = e.WaivedDate,
                        ReturnId = filedReturn?.Id,
                        FiledDate = filedReturn?.SubmittedAt,
                        IsLate = hasFiled && filedReturn.SubmittedAt > e.DueDate
                    };
                }).ToList();

                return dtos;
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

                // Check if period is still open for filing
                var dueDate = period.GetDueDate();
                var eligibility = new ReturnFilingEligibility
                {
                    PeriodName = period.Name,
                    DueDate = dueDate
                };

                // Get expected returns for this period
                var expectedReturns = await _context.ExpectedReturns
                    .Include(e => e.Form)
                    .Where(e => e.SaccoId == saccoId && e.PeriodId == periodId)
                    .ToListAsync();

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

                // Check for missing required forms
                var requiredFormIds = expectedReturns
                    .Where(e => !e.IsWaived)
                    .Select(e => e.FormId)
                    .ToList();

                eligibility.MissingForms = requiredFormIds
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

                // Get expected returns to compare
                var periodIds = returns.Select(r => r.PeriodId).Distinct().ToList();
                var expectedReturns = await _context.ExpectedReturns
                    .Include(e => e.Form)
                    .Where(e => e.SaccoId == saccoId && periodIds.Contains(e.PeriodId))
                    .ToListAsync();

                var expectedByPeriod = expectedReturns
                    .GroupBy(e => e.PeriodId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Map to DTOs
                var summaries = new List<SubmittedReturnSummaryDto>();
                foreach (var ret in returns)
                {
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
                        ApprovalStatus = "Pending" // Get from workflow
                    };

                    // Calculate form counts
                    if (expectedByPeriod.TryGetValue(ret.PeriodId, out var expected))
                    {
                        summary.TotalFormsExpected = expected.Count(e => !e.IsWaived);
                        
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

        public async Task<bool> WaiveReturnAsync(string expectedReturnId, string reason, string waivedBy)
        {
            try
            {
                var expectedReturn = await _context.ExpectedReturns
                    .FirstOrDefaultAsync(e => e.Id == expectedReturnId);

                if (expectedReturn == null)
                    return false;

                expectedReturn.IsWaived = true;
                expectedReturn.WaivedReason = reason;
                expectedReturn.WaivedDate = DateTime.Now;
                expectedReturn.WaivedBy = waivedBy;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiving return");
                return false;
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
    }
}