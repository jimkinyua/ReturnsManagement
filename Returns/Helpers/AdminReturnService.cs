using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission;
using Returns.DTOs.Returns_Submission.NWDT;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class AdminReturnService : IAdminReturnService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<AdminReturnService> _logger;
        private readonly IComplianceService _complianceService;


        public AdminReturnService(ReturnsDbContext context, ILogger<AdminReturnService> logger, IComplianceService complianceService)
        {
            _context = context;
            _logger = logger;
            _complianceService = complianceService;
        }

        public async Task<List<AdminGroupedReturnDTO>> GetGroupedReturnsAsync(AdminReturnFilterDTO filter)
        {
            try
            {
                var results = new List<AdminGroupedReturnDTO>();

                // 1. Get grouped returns (based on rating definitions)
                var groupedResults = await GetGroupedReturnsByRatingDefinitionsAsync(filter);
                results.AddRange(groupedResults);

                // 2. Get standalone returns (not part of any rating definition)
                var standaloneResults = await GetStandaloneReturnsAsync(filter);
                results.AddRange(standaloneResults);

                // Apply completion filter
                if (filter.IsComplete.HasValue)
                {
                    results = results.Where(r => r.IsComplete == filter.IsComplete.Value).ToList();
                }

                // Apply pagination
                var skip = (filter.Page - 1) * filter.PageSize;
                results = results.Skip(skip).Take(filter.PageSize).ToList();

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grouped returns");
                throw;
            }
        }

        private async Task<List<AdminGroupedReturnDTO>> GetGroupedReturnsByRatingDefinitionsAsync(AdminReturnFilterDTO filter)
        {
            // Start with rating definitions - ONLY get CAMELS (contains everything)
            var query = _context.RatingDefinations
                .Include(rd => rd.RatingForms)
                .Where(rd => rd.RatingName == "CAELS") // Only CAMELS rating definitions
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SaccoType))
            {
                query = query.Where(rd => rd.SaccoType == filter.SaccoType);
            }

            if (!string.IsNullOrEmpty(filter.RatingDefinitionId))
            {
                query = query.Where(rd => rd.Id == filter.RatingDefinitionId);
            }

            var ratingDefinitions = await query.ToListAsync();
            var results = new List<AdminGroupedReturnDTO>();

            foreach (var ratingDef in ratingDefinitions)
            {
                // Get all submissions for this rating definition and filter by submission date
                var submissionsQuery = _context.ReturnSubmissions
                    .Include(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.Period)
                    .ThenInclude(p => p.FrequencyCatalog)
                    .Include(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.Period)
                    .ThenInclude(p => p.ReportingYear)
                    .Include(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.ReturnForm)
                    //.Where(rs => rs.Status == ExpectedStatus.Filed) // Only get Filed status submissions
                    .AsQueryable();

                // Filter by submission date instead of period date
                if (filter.Year.HasValue)
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.SubmittedAt.Year == filter.Year.Value);
                }

                if (filter.Month.HasValue)
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.SubmittedAt.Month == filter.Month.Value);
                }

                if (!string.IsNullOrEmpty(filter.Frequency))
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.ExpectedReturn.Period.FrequencyCatalog.Name == filter.Frequency);
                }

                if (!string.IsNullOrEmpty(filter.PeriodId))
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.ExpectedReturn.PeriodId == filter.PeriodId);
                }

                var allSubmissions = await submissionsQuery.ToListAsync();

                // Group by period and then by SACCO
                var periodGroups = allSubmissions.GroupBy(s => s.ExpectedReturn.PeriodId);

                foreach (var periodGroup in periodGroups)
                {
                    var period = periodGroup.First().ExpectedReturn.Period;
                    var submissions = periodGroup.ToList();

                    // Group by SACCO
                    var saccoGroups = submissions.GroupBy(s => s.SaccoId);

                    foreach (var saccoGroup in saccoGroups)
                    {
                        var saccoId = saccoGroup.Key;
                        var saccoSubmissions = saccoGroup.ToList();

                        // Get SACCO details
                        var saccoDetails = await GetSaccoDetailsAsync(saccoId);

                        // Check which forms are required for this rating definition
                        var requiredFormCodes = ratingDef.RatingForms.Select(rf => rf.FormCode).ToList();
                        var submittedFormCodes = saccoSubmissions
                            .Select(s => s.ExpectedReturn.ReturnForm.Code)
                            .ToList();

                        var isComplete = requiredFormCodes.All(required => submittedFormCodes.Contains(required));

                        var groupedReturn = new AdminGroupedReturnDTO
                        {
                            GroupId = ratingDef.Id,
                            SaccoId = saccoId,
                            SaccoName = saccoDetails?.SaccoName ?? "Unknown SACCO",
                            PeriodId = period.Id,
                            PeriodName = period.Name,
                            Year = period.ReportingYear.Year,
                            Frequency = period.FrequencyCatalog.Name,
                            StartDate = period.StartDate,
                            EndDate = period.EndDate,
                            SubmittedAt = saccoSubmissions.Max(s => s.SubmittedAt),
                            Status = GetGroupStatus(saccoSubmissions),
                            IsComplete = isComplete,
                            SubmittedForms = submittedFormCodes.Count,
                            Forms = await BuildFormListAsync(saccoSubmissions, requiredFormCodes, period),
                            GroupType = ReturnGroupType.Grouped,
                            GroupName = $"{period.Name} {period.ReportingYear.Year} Returns - {saccoDetails?.SaccoName ?? "Unknown SACCO"}"
                        };

                        results.Add(groupedReturn);
                    }
                }
            }

            return results;
        }

        private async Task<List<AdminGroupedReturnDTO>> GetStandaloneReturnsAsync(AdminReturnFilterDTO filter)
        {
            var results = new List<AdminGroupedReturnDTO>();

            // Get all forms that are not part of any rating definition
            var formsInRatingDefinitions = await _context.RatingForms
                .Select(rf => rf.FormCode)
                .Distinct()
                .ToListAsync();

            // Get all return forms that are not in rating definitions
            var standaloneForms = await _context.ReturnForms
                .Where(rf => !formsInRatingDefinitions.Contains(rf.Code))
                .ToListAsync();

            // Get all standalone submissions and filter by submission date
            var submissionsQuery = _context.ReturnSubmissions
                .Include(rs => rs.ExpectedReturn)
                .ThenInclude(er => er.Period)
                .ThenInclude(p => p.FrequencyCatalog)
                .Include(rs => rs.ExpectedReturn)
                .ThenInclude(er => er.Period)
                .ThenInclude(p => p.ReportingYear)
                .Include(rs => rs.ExpectedReturn)
                .ThenInclude(er => er.ReturnForm)
                .Where(rs => standaloneForms.Select(f => f.Id).Contains(rs.ExpectedReturn.ReturnFormId))
                //.Where(rs => rs.Status == "Filed") // Only get Filed status submissions
                .AsQueryable();

            // Filter by submission date instead of period date
            if (filter.Year.HasValue)
            {
                submissionsQuery = submissionsQuery.Where(rs => rs.SubmittedAt.Year == filter.Year.Value);
            }

            if (filter.Month.HasValue)
            {
                submissionsQuery = submissionsQuery.Where(rs => rs.SubmittedAt.Month == filter.Month.Value);
            }

            if (!string.IsNullOrEmpty(filter.Frequency))
            {
                submissionsQuery = submissionsQuery.Where(rs => rs.ExpectedReturn.Period.FrequencyCatalog.Name == filter.Frequency);
            }

            if (!string.IsNullOrEmpty(filter.PeriodId))
            {
                submissionsQuery = submissionsQuery.Where(rs => rs.ExpectedReturn.PeriodId == filter.PeriodId);
            }

            var allSubmissions = await submissionsQuery.ToListAsync();

            // Group by period and then by form and SACCO
            var periodGroups = allSubmissions.GroupBy(s => s.ExpectedReturn.PeriodId);

            foreach (var periodGroup in periodGroups)
            {
                var period = periodGroup.First().ExpectedReturn.Period;
                var periodSubmissions = periodGroup.ToList();

                // Group by form and then by SACCO
                var formGroups = periodSubmissions.GroupBy(s => s.ExpectedReturn.ReturnFormId);

                foreach (var formGroup in formGroups)
                {
                    var form = formGroup.First().ExpectedReturn.ReturnForm;
                    var formSubmissions = formGroup.ToList();

                    // Group by SACCO
                    var saccoGroups = formSubmissions.GroupBy(s => s.SaccoId);

                    foreach (var saccoGroup in saccoGroups)
                    {
                        var saccoId = saccoGroup.Key;
                        var saccoSubmissions = saccoGroup.ToList();

                        if (!saccoSubmissions.Any()) continue;

                        var saccoDetails = await GetSaccoDetailsAsync(saccoId);
                        var submission = saccoSubmissions.First();

                        var standaloneReturn = new AdminGroupedReturnDTO
                        {
                            GroupId = "standalone",
                            SaccoId = saccoId,
                            SaccoName = saccoDetails?.SaccoName ?? "Unknown SACCO",
                            PeriodId = period.Id,
                            PeriodName = period.Name,
                            Year = period.ReportingYear.Year,
                            Frequency = period.FrequencyCatalog.Name,
                            StartDate = period.StartDate,
                            EndDate = period.EndDate,
                            SubmittedAt = submission.SubmittedAt,
                            Status = GetGroupStatus(saccoSubmissions),
                            IsComplete = saccoSubmissions.Any(), // Standalone returns are complete if submitted
                            SubmittedForms = saccoSubmissions.Any() ? 1 : 0,
                            Forms = await BuildStandaloneFormListAsync(saccoSubmissions, period),
                            GroupType = ReturnGroupType.Standalone,
                            GroupName = $"{period.Name} {period.ReportingYear.Year} Returns - {saccoDetails?.SaccoName ?? "Unknown SACCO"}"
                        };

                        results.Add(standaloneReturn);
                    }
                }
            }

            return results;
        }

        public async Task<AdminGroupedReturnDetailsDTO> GetGroupedReturnDetailsAsync(string groupId, string periodId, string saccoId)
        {
            try
            {
                var period = await _context.ReturnPeriods
                    .Include(p => p.FrequencyCatalog)
                    .Include(p => p.ReportingYear)
                    .FirstOrDefaultAsync(p => p.Id == periodId);

                if (period == null)
                    throw new ArgumentException("Period not found");

                var saccoDetails = await GetSaccoDetailsAsync(saccoId);
                var saccoType = saccoDetails?.SaccoType ?? "0";

                var detailsDto = new AdminGroupedReturnDetailsDTO
                {
                    GroupId = groupId,
                    GroupName = $"{period.Name} {period.ReportingYear.Year} Returns - {saccoDetails?.SaccoName ?? "Unknown SACCO"}",
                    SaccoId = saccoId,
                    SaccoName = saccoDetails?.SaccoName ?? "Unknown SACCO",
                    PeriodId = period.Id,
                    PeriodName = period.Name,
                    Year = period.ReportingYear.Year,
                    Frequency = period.FrequencyCatalog.Name,
                    StartDate = period.StartDate,
                    EndDate = period.EndDate,
                    Status = "",
                    IsComplete = false,
                    SubmittedForms = 0,
                    GroupType = ReturnGroupType.Grouped
                };

                if (groupId == "standalone")
                {
                    var submissionsR = await _context.ReturnSubmissions
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                        .Where(rs => rs.ExpectedReturn.PeriodId == periodId && rs.SaccoId == saccoId)
                        .ToListAsync();

                    if (!submissionsR.Any())
                        throw new ArgumentException("No submissions found for this SACCO and period");

                    var submission = submissionsR.First();
                    detailsDto.Status = GetGroupStatus(submissionsR);
                    detailsDto.IsComplete = submissionsR.Any();
                    detailsDto.SubmittedForms = submissionsR.Any() ? 1 : 0;
                    detailsDto.GroupType = ReturnGroupType.Standalone;
                    detailsDto.SubmittedAt = submission.SubmittedAt;

                    // Only fill the relevant property based on category
                    var form = submission.ExpectedReturn.ReturnForm;
                    switch (form.Category)
                    {
                        case FormCategory.CapitalAdequacy:
                            detailsDto.CapitalAdequacy = MapCapitalAdequacy(submission, saccoType);
                            break;
                        case FormCategory.LiquidityStatement:
                            detailsDto.Liquidity = MapLiquidity(submission, saccoType);
                            break;
                        case FormCategory.RiskClassification:
                            detailsDto.RiskClassification = MapRiskClassification(submission, saccoType);
                            break;
                        case FormCategory.DepositReturn:
                            //detailsDto.DepositReturn = MapDepositReturn(submission, saccoType);
                            break;
                        case FormCategory.FinancialPosition:
                            //detailsDto.FinancialPosition = MapFinancialPosition(submission, saccoType);
                            break;
                        case FormCategory.StatementOfComprehensiveIncome:
                            //detailsDto.ComprehensiveIncome = MapComprehensiveIncome(submission, saccoType);
                            break;
                            // Add more as needed
                    }
                    return detailsDto;
                }
                else
                {
                    var ratingDef = await _context.RatingDefinations
                        .Include(rd => rd.RatingForms)
                        .FirstOrDefaultAsync(rd => rd.Id == groupId);

                    if (ratingDef == null)
                        throw new ArgumentException("Rating definition not found");

                    var submissions = await _context.ReturnSubmissions
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                        .Where(rs => rs.ExpectedReturn.PeriodId == periodId && rs.SaccoId == saccoId)
                        .ToListAsync();

                    var requiredFormCodes = ratingDef.RatingForms.Select(rf => rf.FormCode).ToList();
                    var submittedFormCodes = submissions.Select(s => s.ExpectedReturn.ReturnForm.Code).ToList();
                    var isComplete = requiredFormCodes.All(required => submittedFormCodes.Contains(required));
                    detailsDto.IsComplete = isComplete;
                    detailsDto.SubmittedForms = submittedFormCodes.Count;
                    detailsDto.Status = GetGroupStatus(submissions);
                    detailsDto.SubmittedAt = submissions.Any() ? submissions.Max(s => s.SubmittedAt) : DateTime.MinValue;
                    detailsDto.GroupType = ReturnGroupType.Grouped;

                    foreach (var submission in submissions)
                    {
                        var form = submission.ExpectedReturn.ReturnForm;
                        switch (form.Category)
                        {
                            case FormCategory.CapitalAdequacy:
                                detailsDto.CapitalAdequacy = MapCapitalAdequacy(submission, saccoType);
                                break;
                            case FormCategory.LiquidityStatement:
                                detailsDto.Liquidity = MapLiquidity(submission, saccoType);
                                break;
                            case FormCategory.RiskClassification:
                                detailsDto.RiskClassification = MapRiskClassification(submission, saccoType);
                                break;
                            case FormCategory.DepositReturn:
                                //detailsDto.DepositReturn = MapDepositReturn(submission, saccoType);
                                break;
                            case FormCategory.FinancialPosition:
                                //detailsDto.FinancialPosition = MapFinancialPosition(submission, saccoType);
                                break;
                            case FormCategory.StatementOfComprehensiveIncome:
                                //detailsDto.ComprehensiveIncome = MapComprehensiveIncome(submission, saccoType);
                                break;
                                // Add more as needed
                        }
                    }
                    return detailsDto;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grouped return details");
                throw;
            }
        }

        public async Task<List<string>> GetAvailableYearsAsync()
        {
            return await _context.ReportingYears
                .Select(ry => ry.Year.ToString())
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        public async Task<List<string>> GetAvailableSaccoTypesAsync()
        {
            return await _context.RatingDefinations
                .Select(rd => rd.SaccoType)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetAvailableFrequenciesAsync()
        {
            return await _context.FrequencyCatalogs
                .Where(fc => fc.IsActive)
                .Select(fc => fc.Name)
                .Distinct()
                .ToListAsync();
        }

        private async Task<dynamic?> GetSaccoDetailsAsync(string saccoId)
        {
            try
            {
                var sacco = await _complianceService.GetSaccoByIdAsync(saccoId);
                // This would typically call your compliance service
                // For now, return a simple object
                return sacco;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get SACCO details for {SaccoId}", saccoId);
                return new { SaccoName = $"SACCO {saccoId}" };
            }
        }

        private string GetGroupStatus(List<ReturnSubmission> submissions)
        {
            if (!submissions.Any())
                return "No Submissions";

            var latestSubmission = submissions.Max(s => s.SubmittedAt);
            var now = DateTime.Now;

            if (latestSubmission > now)
                return "On Time";
            else
                return "Late";
        }

        private async Task<List<GroupedReturnFormDTO>> BuildFormListAsync(
            List<ReturnSubmission> submissions,
            List<string> requiredFormCodes,
            ReturnPeriods period)
        {
            var forms = new List<GroupedReturnFormDTO>();

            foreach (var formCode in requiredFormCodes)
            {
                var submission = submissions.FirstOrDefault(s => s.ExpectedReturn.ReturnForm.Code == formCode);
                var isSubmitted = submission != null;
                var isLate = false;
                var daysLate = 0;

                if (isSubmitted)
                {
                    var deadline = period.FilingDeadline;
                    isLate = submission.SubmittedAt > deadline;
                    daysLate = isLate ? (int)(submission.SubmittedAt - deadline).TotalDays : 0;
                }

                forms.Add(new GroupedReturnFormDTO
                {
                    FormId = submission?.ExpectedReturn.ReturnFormId ?? "",
                    FormCode = formCode,
                    FormName = submission?.ExpectedReturn.ReturnForm.FormName ?? $"Form {formCode}",
                    SubmissionId = submission?.Id ?? "",
                    SubmittedAt = submission?.SubmittedAt,
                    Status = submission?.Status ?? "Not Submitted",
                    IsSubmitted = isSubmitted,
                    IsLate = isLate,
                    DaysLate = daysLate
                });
            }

            return forms;
        }

        private async Task<List<GroupedReturnFormDTO>> BuildStandaloneFormListAsync(
            List<ReturnSubmission> submissions,
            ReturnPeriods period)
        {
            var forms = new List<GroupedReturnFormDTO>();

            foreach (var submission in submissions)
            {
                var isLate = false;
                var daysLate = 0;

                if (submission.SubmittedAt != default)
                {
                    var deadline = period.FilingDeadline;
                    isLate = submission.SubmittedAt > deadline;
                    daysLate = isLate ? (int)(submission.SubmittedAt - deadline).TotalDays : 0;
                }

                forms.Add(new GroupedReturnFormDTO
                {
                    FormId = submission.ExpectedReturn.ReturnFormId,
                    FormCode = submission.ExpectedReturn.ReturnForm.Code,
                    FormName = submission.ExpectedReturn.ReturnForm.FormName,
                    SubmissionId = submission.Id,
                    SubmittedAt = submission.SubmittedAt,
                    Status = submission.Status,
                    IsSubmitted = true,
                    IsLate = isLate,
                    DaysLate = daysLate
                });
            }

            return forms;
        }

        private object MapCapitalAdequacy(ReturnSubmission submission, string? saccoType)
        {
            if (saccoType == "0") // DT
            {
                var entity = submission.DTCapitalAdequacyReturns.FirstOrDefault();
                if (entity == null) return null;
                return new Returns.DTOs.Returns_Submission.DT.CapitalAdequacyDTO
                {
                    ShareCapital = entity.ShareCapital,
                    StatutoryReserves = entity.StatutoryReserves,
                    RetainedEarningsAccumulatedLosses = entity.RetainedEarningsAccumulatedLosses,
                    NetSurplusAfterTaxCurrentYearToDate = entity.NetSurplusAfterTaxCurrentYearToDate,
                    CapitalGrantsEquityInNature = entity.CapitalGrantsEquityInNature,
                    GeneralReserves = entity.GeneralReserves,
                    OtherReserves = entity.OtherReserves,
                    SubTotalCoreCapital = entity.SubTotalCoreCapital,
                    InvestmentsInSubsidiaryAndEquityInstruments = entity.InvestmentsInSubsidiaryAndEquityInstruments,
                    OtherDeductions = entity.OtherDeductions,
                    TotalDeductions = entity.TotalDeductions,
                    CoreCapital = entity.CoreCapital,
                    InstitutionalCapital = entity.InstitutionalCapital,
                    CashLocalAndForeignCurrency = entity.CashLocalAndForeignCurrency,
                    GovernmentSecurities = entity.GovernmentSecurities,
                    DepositsAndBalancesAtOtherInstitutions = entity.DepositsAndBalancesAtOtherInstitutions,
                    LoansAndAdvances = entity.LoansAndAdvances,
                    Investments = entity.Investments,
                    PropertyAndEquipment = entity.PropertyAndEquipment,
                    OtherAssets = entity.OtherAssets,
                    TotalOnBalanceSheetAssets = entity.TotalOnBalanceSheetAssets,
                    TotalAssetsPerBalanceSheet = entity.TotalAssetsPerBalanceSheet,
                    Difference = entity.Difference,
                    CoreCapitalToAssetsRatio = entity.CoreCapitalToAssetsRatio,
                    CoreCapitalToAssetsRatioExcessDeficiency = entity.CoreCapitalToAssetsRatioExcessDeficiency,
                    InstitutionalCapitalToAssetsRatio = entity.InstitutionalCapitalToAssetsRatio,
                    InstitutionalCapitalToAssetsRatioExcessDeficiency = entity.InstitutionalCapitalToAssetsRatioExcessDeficiency,
                    CoreCapitalToDepositsRatio = entity.CoreCapitalToDepositsRatio,
                    CoreCapitalToDepositsRatioExcessDeficiency = entity.CoreCapitalToDepositsRatioExcessDeficiency,
                    TotalOffBalanceSheetAssets = entity.TotalOffBalanceSheetAssets,
                    TotalAssets = entity.TotalAssets,
                    TotalDepositsLiabilities = entity.TotalDepositsLiabilities,
                    MinimumCoreCapitalToAssetsRatio = entity.MinimumCoreCapitalToAssetsRatio,
                    MinimumInstitutionalToAssetsRatio = entity.MinimumInstitutionalToAssetsRatio,
                    MinimumCoreCapitalToDepositsRatio = entity.MinimumCoreCapitalToDepositsRatio,
                };
            }
            else // NWDT
            {
                var entity = submission.NWDTCapitalAdequacyReturns.FirstOrDefault();
                if (entity == null) return null;
                return new NWDTCapitalAdequacyDTO
                {
                    StartDate = entity.StartDate,
                    EndDate = entity.EndDate,
                    DaysLateBy = entity.DaysLateBy,
                    ShareCapital = entity.ShareCapital,
                    CapitalGrants = entity.CapitalGrants,
                    StatutoryReserves = entity.StatutoryReserves,
                    RetainedEarningsAccumulatedLosses = entity.RetainedEarningsAccumulatedLosses,
                    NetSurplusAfterTaxCurrentYearToDate = entity.NetSurplusAfterTaxCurrentYearToDate,
                    CapitalGrantsEquityInNature = entity.CapitalGrantsEquityInNature,
                    GeneralReserves = entity.GeneralReserves,
                    OtherReserves = entity.OtherReserves,
                    SubTotalCoreCapital = entity.SubTotalCoreCapital,
                    InvestmentsInSubsidiaryAndEquityInstruments = entity.InvestmentsInSubsidiaryAndEquityInstruments,
                    OtherDeductions = entity.OtherDeductions,
                    TotalDeductions = entity.TotalDeductions,
                    CoreCapital = entity.CoreCapital,
                    RetainedEarningsAndDisclosedReserves = entity.RetainedEarningsAndDisclosedReserves,
                    CashLocalAndForeignCurrency = entity.CashLocalAndForeignCurrency,
                    GovernmentSecurities = entity.GovernmentSecurities,
                    DepositsAndBalancesAtOtherInstitutions = entity.DepositsAndBalancesAtOtherInstitutions,
                    LoansAndAdvances = entity.LoansAndAdvances,
                    Investments = entity.Investments,
                    PropertyAndEquipment = entity.PropertyAndEquipment,
                    OtherAssets = entity.OtherAssets,
                    TotalOnBalanceSheetAssets = entity.TotalOnBalanceSheetAssets,
                    TotalAssetsPerBalanceSheet = entity.TotalAssetsPerBalanceSheet,
                    Difference = entity.Difference,
                    TotalOffBalanceSheetAssets = entity.TotalOffBalanceSheetAssets,
                    TotalAssets = entity.TotalAssets,
                    TotalDepositsLiabilitiesPerBalanceSheet = entity.TotalDepositsLiabilitiesPerBalanceSheet,
                    CoreCapitalToAssetsRatio = entity.CoreCapitalToAssetsRatio,
                    MinimumCoreCapitalToAssetsRatioRequirement = entity.MinimumCoreCapitalToAssetsRatioRequirement,
                    MinimumRetainedEarningsAndDisclosedReservesToCoreCapitalRequirement = entity.MinimumRetainedEarningsAndDisclosedReservesToCoreCapitalRequirement,
                    MinimumCoreCapitalToDepositsRatioRequirement = entity.MinimumCoreCapitalToDepositsRatioRequirement,
                    CoreCapitalToAssetsRatioExcessDeficiency = entity.CoreCapitalToAssetsRatioExcessDeficiency,
                    RetainedEarningsAndDisclosedReservesToCoreCapital = entity.RetainedEarningsAndDisclosedReservesToCoreCapital,
                    RetainedEarningsAndDisclosedReservesToCoreCapitalExcessDeficiency = entity.RetainedEarningsAndDisclosedReservesToCoreCapitalExcessDeficiency,
                    CoreCapitalToDepositsRatio = entity.CoreCapitalToDepositsRatio,
                    CoreCapitalToDepositsRatioExcessDeficiency = entity.CoreCapitalToDepositsRatioExcessDeficiency,

                };
            }
        }

        private object MapLiquidity(ReturnSubmission submission, string? saccoType)
        {
            if (saccoType == "0") // DT
            {
                var liquidityEntity = submission.DTLiquidityReturns.FirstOrDefault();
                if (liquidityEntity == null) return null;
                return new DTOs.Returns_Submission.DT.LiquidityStatementDTO
                {
                    RequiresResubmission = liquidityEntity.RequiresResubmission,
                    LocalNotesAndCoins = liquidityEntity.LocalNotesAndCoins,
                    ForeignNotesAndCoins = liquidityEntity.ForeignNotesAndCoins,
                    TotalNotesAndCoins = liquidityEntity.TotalNotesAndCoins,

                    BalancesWithCommercialBanks = liquidityEntity.BalancesWithCommercialBanks,
                    TimeDepositsWithBanksMoreThan90Days = liquidityEntity.TimeDepositsWithBanksMoreThan90Days,
                    OverdraftsAndMaturedLoans = liquidityEntity.OverdraftsAndMaturedLoans,

                    BalancesWithOtherSaccoSocieties = liquidityEntity.BalancesWithOtherSaccoSocieties,
                    BalancesWithOtherFinancialInstitutions = liquidityEntity.BalancesWithOtherFinancialInstitutions,
                    BalancesDueToOtherSaccoSocieties = liquidityEntity.BalancesDueToOtherSaccoSocieties,
                    BalancesDueToFinancialInstitutions = liquidityEntity.BalancesDueToFinancialInstitutions,
                    MaturedLoansFromFinancialInstitutions = liquidityEntity.MaturedLoansFromFinancialInstitutions,

                    TreasuryBills = liquidityEntity.TreasuryBills,
                    TreasuryBonds = liquidityEntity.TreasuryBonds,

                    NetLiquidAssets = liquidityEntity.NetLiquidAssets,

                    DepositsFromMembers = liquidityEntity.DepositsFromMembers,
                    DepositsFromOtherSources = liquidityEntity.DepositsFromOtherSources,
                    TotalDeposits = liquidityEntity.TotalDeposits,
                    BalancesDueToSaccos = liquidityEntity.BalancesDueToSaccos,
                    BalancesDueToBanks = liquidityEntity.BalancesDueToBanks,
                    BalancesDueToOtherFinancialInst = liquidityEntity.BalancesDueToOtherFinancialInst,
                    TotalDeductions = liquidityEntity.TotalDeductions,
                    NetDepositLiabilities = liquidityEntity.NetDepositLiabilities,

                    MaturedLiabilities = liquidityEntity.MaturedLiabilities,
                    LiabilitiesMaturing91Days = liquidityEntity.LiabilitiesMaturing91Days,
                    TotalOtherLiabilities = liquidityEntity.TotalOtherLiabilities,

                    TotalShortTermLiabilities = liquidityEntity.TotalShortTermLiabilities,
                    LiquidityRatio = liquidityEntity.LiquidityRatio,
                    MinimumLiquidityRequirement = liquidityEntity.MinimumLiquidityRequirement,
                    LiquidityRatioExcessDeficit = liquidityEntity.LiquidityRatioExcessDeficit,
                    Year = liquidityEntity.Year,
                    StartDate = liquidityEntity.StartDate,
                    EndDate = liquidityEntity.EndDate
                };
            }
            else // NWDT
            {
                var entity = submission.NWDTLiquidityReturns.FirstOrDefault();
                if (entity == null) return null;
                return new NWDTLiquidityStatementDTO
                {
                    StartDate = entity.StartDate,
                    EndDate = entity.EndDate,
                    Period = entity.Period,
                    DaysLateBy = entity.DaysLateBy,
                    LocalNotesAndCoins = entity.LocalNotesAndCoins,
                    ForeignNotesAndCoins = entity.ForeignNotesAndCoins,

                    BalancesWithCommercialBanks = entity.BalancesWithCommercialBanks,
                    TimeDepositsWithBanksMoreThan90Days = entity.TimeDepositsWithBanksMoreThan90Days,
                    OverdraftsAndMaturedLoans = entity.OverdraftsAndMaturedLoans,

                    BalancesWithOtherSaccoSocieties = entity.BalancesWithOtherSaccoSocieties,
                    BalancesWithOtherFinancialInstitutions = entity.BalancesWithOtherFinancialInstitutions,
                    BalancesDueToOtherSaccoSocieties = entity.BalancesDueToOtherSaccoSocieties,
                    BalancesDueToFinancialInstitutions = entity.BalancesDueToFinancialInstitutions,
                    MaturedLoansAndAdvances = entity.MaturedLoansAndAdvances,

                    TreasuryBills = entity.TreasuryBills,
                    TreasuryBondsBearerBonds = entity.TreasuryBondsBearerBonds,

                    MaturedLiabilities = entity.MaturedLiabilities,
                    LiabilitiesMaturing91Days = entity.LiabilitiesMaturing91Days,
                    TotalOtherLiabilities = entity.TotalOtherLiabilities,

                    NetLiquidAssets = entity.NetLiquidAssets,
                    TotalShortTermLiabilities = entity.TotalShortTermLiabilities,
                    LiquidityRatio = entity.LiquidityRatio,
                    MinimumLiquidityRequirement = entity.MinimumLiquidityRequirement,
                    LiquidityRatioExcessDeficit = entity.LiquidityRatioExcessDeficit
                };
            }
        }

        private object MapRiskClassification(ReturnSubmission submission, string? saccoType)
        {
            if (saccoType == "0") // DT
            {
                var entity = submission.DTRiskClassificationReturns.FirstOrDefault();
                if (entity == null) return null;
                return new Returns.DTOs.Returns_Submission.DT.RiskClassificationDTO
                {
                    /*   TotalAssets = entity.TotalAssets,
                       TotalLiabilities = entity.TotalLiabilities,
                       NetWorth = entity.NetWorth,
                       TotalRevenue = entity.TotalRevenue,
                       TotalExpenses = entity.TotalExpenses,
                       NetProfit = entity.NetProfit,
                       FilePath = entity.FilePath*/
                };
            }
            else // NWDT
            {
                var entity = submission.NWDTRiskClassificationReturns.FirstOrDefault();
                if (entity == null) return null;
                return new Returns.DTOs.Returns_Submission.NWDT.NWDTRiskClassificationDTO
                {
                    /*    TotalAssets = entity.TotalAssets,
                        TotalLiabilities = entity.TotalLiabilities,
                        NetWorth = entity.NetWorth,
                        TotalRevenue = entity.TotalRevenue,
                        TotalExpenses = entity.TotalExpenses,
                        NetProfit = entity.NetProfit,
                        FilePath = entity.FilePath*/
                };
            }
        }

        /* private object MapDepositReturn(ReturnSubmission submission, string? saccoType)
         {
             if (saccoType == "0") // DT
             {
                 //var entity = submissiondepos.FirstOrDefault();
                 if (entity == null) return null;
                 return new Returns.DTOs.Returns_Submission.DT.DepositReturnDTO
                 {
                     TotalDeposits = entity.TotalDeposits,
                     SavingsDeposits = entity.SavingsDeposits,
                     CurrentDeposits = entity.CurrentDeposits,
                     TimeDeposits = entity.TimeDeposits,
                     TotalDepositLiabilities = entity.TotalDepositLiabilities,
                     SavingsDepositLiabilities = entity.SavingsDepositLiabilities,
                     CurrentDepositLiabilities = entity.CurrentDepositLiabilities,
                     TimeDepositLiabilities = entity.TimeDepositLiabilities,
                     FilePath = entity.FilePath
                 };
             }
             else // NWDT
             {
                 var entity = submission.NWDTDepositReturns.FirstOrDefault();
                 if (entity == null) return null;
                 return new Returns.DTOs.Returns_Submission.NWDT.NWDTDepositReturnDTO
                 {
                     TotalDeposits = entity.TotalDeposits,
                     SavingsDeposits = entity.SavingsDeposits,
                     CurrentDeposits = entity.CurrentDeposits,
                     TimeDeposits = entity.TimeDeposits,
                     TotalDepositLiabilities = entity.TotalDepositLiabilities,
                     SavingsDepositLiabilities = entity.SavingsDepositLiabilities,
                     CurrentDepositLiabilities = entity.CurrentDepositLiabilities,
                     TimeDepositLiabilities = entity.TimeDepositLiabilities,
                     FilePath = entity.FilePath
                 };
             }
         }*/

        /*private object MapFinancialPosition(ReturnSubmission submission, string? saccoType)
        {
            if (saccoType == "0") // DT
            {
                var entity = submission.DTFinancialPositionReturns.FirstOrDefault();
                if (entity == null) return null;
                return new Returns.DTOs.Returns_Submission.DT.FinancialPositionDTO
                {
                    TotalAssets = entity.TotalAssets,
                    TotalLiabilities = entity.TotalLiabilities,
                    NetWorth = entity.NetWorth,
                    TotalRevenue = entity.TotalRevenue,
                    TotalExpenses = entity.TotalExpenses,
                    NetProfit = entity.NetProfit,
                    FilePath = entity.FilePath
                };
            }
            else // NWDT
            {
                var entity = submission.NWDTDepositReturns.FirstOrDefault(); // Assuming NWDT uses the same DTO for Financial Position
                if (entity == null) return null;
                return new Returns.DTOs.Returns_Submission.NWDT.NWDTDepositReturnDTO
                {
                    TotalDeposits = entity.TotalDeposits,
                    SavingsDeposits = entity.SavingsDeposits,
                    CurrentDeposits = entity.CurrentDeposits,
                    TimeDeposits = entity.TimeDeposits,
                    TotalDepositLiabilities = entity.TotalDepositLiabilities,
                    SavingsDepositLiabilities = entity.SavingsDepositLiabilities,
                    CurrentDepositLiabilities = entity.CurrentDepositLiabilities,
                    TimeDepositLiabilities = entity.TimeDepositLiabilities,
                    FilePath = entity.FilePath
                };
            }
        }*/

        /*  private object MapComprehensiveIncome(ReturnSubmission submission, string? saccoType)
          {
              if (saccoType == "0") // DT
              {
                  var entity = submission.DTComprehensiveIncomeReturns.FirstOrDefault();
                  if (entity == null) return null;
                  return new Returns.DTOs.Returns_Submission.DT.ComprehensiveIncomeDTO
                  {
                      TotalRevenue = entity.TotalRevenue,
                      TotalExpenses = entity.TotalExpenses,
                      NetProfit = entity.NetProfit,
                      FilePath = entity.FilePath
                  };
              }
              else // NWDT
              {
                  var entity = submission.NWDTDepositReturns.FirstOrDefault(); // Assuming NWDT uses the same DTO for Comprehensive Income
                  if (entity == null) return null;
                  return new Returns.DTOs.Returns_Submission.NWDT.NWDTDepositReturnDTO
                  {
                      TotalDeposits = entity.TotalDeposits,
                      SavingsDeposits = entity.SavingsDeposits,
                      CurrentDeposits = entity.CurrentDeposits,
                      TimeDeposits = entity.TimeDeposits,
                      TotalDepositLiabilities = entity.TotalDepositLiabilities,
                      SavingsDepositLiabilities = entity.SavingsDepositLiabilities,
                      CurrentDepositLiabilities = entity.CurrentDepositLiabilities,
                      TimeDepositLiabilities = entity.TimeDepositLiabilities,
                      FilePath = entity.FilePath
                  };
              }
          }*/
    }
}