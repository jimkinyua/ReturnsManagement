using iText.Commons.Utils;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs;
using Returns.DTOs.Returns.Returns_Analysis;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns.Returns_Submission.DT;
using Returns.DTOs.Returns_Analysis;
using Returns.DTOs.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;
using Returns.DTOs.Returns_Submission.NWDT;
using Returns.DTOs.Returns_Submission.Returns_Submission.DT;
using Returns.DTOs.WorkFlow_Engine;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System.Text.Json;
using static Returns.Helpers.ReturnAnalysisHelper;
using static Returns.Helpers.TokenHelper;

namespace Returns.Helpers
{
    public class AdminReturnService : IAdminReturnService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<AdminReturnService> _logger;
        private readonly IComplianceService _complianceService;
        private readonly IConfiguration _configuration;
        private readonly IWorkflowEngineService _workflowEngineService;



        public AdminReturnService(ReturnsDbContext context, IWorkflowEngineService workflowEngineService, ILogger<AdminReturnService> logger, IComplianceService complianceService, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _complianceService = complianceService;
            _configuration = configuration;
            _workflowEngineService = workflowEngineService;
        }

        public async Task<List<AdminGroupedReturnDTO>> GetSaccoGroupedReturnsAsync(SaccoReturnFilterDTO filter, LoggedInEntity loggedInSacco)
        {
            try
            {
                var results = new List<AdminGroupedReturnDTO>();

                // Fetch all relevant submissions
                var submissionsQuery = _context.ReturnSubmissions
                    .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                            .ThenInclude(p => p.FrequencyCatalog)
                    .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                            .ThenInclude(p => p.ReportingYear)
                    .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                    .Where(rs => rs.Status == SubmissionStatus.Submitted.ToString()
                    && rs.SaccoId == loggedInSacco.SaccoId)
                    .AsQueryable();

                // Apply filters based on DUE DATES (period end dates) instead of submission dates
                if (filter.Year.HasValue)
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.ExpectedReturn.Period.FilingDeadline.Year == filter.Year.Value);
                }
                if (filter.Month.HasValue)
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.ExpectedReturn.FilingDeadline.Month == filter.Month.Value);
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

                // Group by period
                var periodGroups = allSubmissions.GroupBy(s => s.ExpectedReturn.PeriodId);

                foreach (var periodGroup in periodGroups)
                {
                    var period = periodGroup.First().ExpectedReturn.Period;
                    bool isQuarterly = period.FrequencyCatalog.Id == 5;  // QTR

                    if (isQuarterly)
                    {
                        // Quarterly: Group all forms by SACCO
                        var saccoGroups = periodGroup.GroupBy(s => s.SaccoId);

                        foreach (var saccoGroup in saccoGroups)
                        {
                            var saccoSubmissions = saccoGroup.ToList();
                            var saccoDetails = await GetSaccoDetailsAsync(saccoGroup.Key);
                            string saccoType = saccoDetails?.SaccoType ?? "0";

                            // Get all expected Q forms for this period/SACCO type
                            var expectedQForms = await _context.ExpectedReturns
                                .Where(er => er.PeriodId == period.Id && er.ReturnForm.SaccoTypeId == saccoType)
                                .Select(er => er.ReturnForm.Code)
                                .ToHashSetAsync();

                            var filedCodes = saccoSubmissions
                                .Select(s => s.ExpectedReturn.ReturnForm.Code)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

                            // Fetch CAELS rating definition to validate group (optional, for naming)
                            var ratingDef = await _context.RatingDefinations
                                .FirstOrDefaultAsync(rd => rd.RatingName.Contains("CAELS") && rd.SaccoType == saccoType);

                            // Calculate lateness statistics
                            var (hasLateSubmissions, totalDaysLate, lateFormsCount) = CalculateLatenessStats(saccoSubmissions, period);

                            results.Add(new AdminGroupedReturnDTO
                            {
                                GroupId = ratingDef?.Id ?? period.Id,  // Use rating ID or period ID
                                SaccoId = saccoGroup.Key,
                                SaccoName = saccoDetails?.SaccoName ?? "Unknown SACCO",
                                PeriodId = period.Id,
                                PeriodName = period.Name,
                                Year = period.ReportingYear.Year,
                                Frequency = period.FrequencyCatalog.Name,
                                StartDate = period.StartDate,
                                EndDate = period.EndDate,
                                SubmittedAt = saccoSubmissions.Max(s => s.SubmittedAt),
                                Status = GetGroupStatus(saccoSubmissions),
                                IsComplete = expectedQForms.SetEquals(filedCodes),  // All expected Q forms filed
                                SubmittedForms = filedCodes.Count,  // Counts all Q forms (e.g., 7)
                                Forms = await BuildFormListAsync(saccoSubmissions, expectedQForms.ToList(), period),
                                GroupType = ReturnGroupType.Grouped,
                                GroupName = $"{period.Name} {period.ReportingYear.Year} Returns - {saccoDetails?.SaccoName ?? "Unknown SACCO"}",
                                HasLateSubmissions = hasLateSubmissions,
                                TotalDaysLate = totalDaysLate,
                                LateFormsCount = lateFormsCount
                            });
                        }
                    }
                    else
                    {
                        // Non-quarterly: Standalone per form/SACCO/period
                        var saccoFormGroups = periodGroup.GroupBy(s => new { s.SaccoId, s.ExpectedReturn.ReturnFormId });

                        foreach (var saccoFormGroup in saccoFormGroups)
                        {
                            var saccoSubmissions = saccoFormGroup.ToList();
                            var saccoDetails = await GetSaccoDetailsAsync(saccoFormGroup.Key.SaccoId);
                            var form = saccoSubmissions.First().ExpectedReturn.ReturnForm;

                            // Calculate lateness statistics
                            var (hasLateSubmissions, totalDaysLate, lateFormsCount) = CalculateLatenessStats(saccoSubmissions, period);

                            results.Add(new AdminGroupedReturnDTO
                            {
                                GroupId = "standalone",
                                SaccoId = saccoFormGroup.Key.SaccoId,
                                SaccoName = saccoDetails?.SaccoName ?? "Unknown SACCO",
                                PeriodId = period.Id,
                                PeriodName = period.Name,
                                Year = period.ReportingYear.Year,
                                Frequency = period.FrequencyCatalog.Name,
                                StartDate = period.StartDate,
                                EndDate = period.EndDate,
                                SubmittedAt = saccoSubmissions.Max(s => s.SubmittedAt),
                                Status = GetGroupStatus(saccoSubmissions),
                                IsComplete = saccoSubmissions.Any(),
                                SubmittedForms = 1,  // One form per standalone
                                Forms = await BuildStandaloneFormListAsync(saccoSubmissions, period),
                                GroupType = ReturnGroupType.Standalone,
                                GroupName = $"{period.Name} {period.ReportingYear.Year} {form.FormName} - {saccoDetails?.SaccoName ?? "Unknown SACCO"}",
                                HasLateSubmissions = hasLateSubmissions,
                                TotalDaysLate = totalDaysLate,
                                LateFormsCount = lateFormsCount
                            });
                        }
                    }
                }

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


        public async Task<List<AdminGroupedReturnDTO>> GetGroupedReturnsAsync(AdminReturnFilterDTO filter)
        {
            try
            {
                var results = new List<AdminGroupedReturnDTO>();

                // Fetch all relevant submissions
                var submissionsQuery = _context.ReturnSubmissions
                    .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                            .ThenInclude(p => p.FrequencyCatalog)
                    .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                            .ThenInclude(p => p.ReportingYear)
                    .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                    .Where(rs => rs.Status == SubmissionStatus.Submitted.ToString())
                    .AsQueryable();

                // Apply filters based on DUE DATES (period end dates) instead of submission dates
                if (filter.Year.HasValue)
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.ExpectedReturn.Period.FilingDeadline.Year == filter.Year.Value);
                }
                if (filter.Month.HasValue)
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.ExpectedReturn.FilingDeadline.Month == filter.Month.Value);
                }
                if (!string.IsNullOrEmpty(filter.Frequency))
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.ExpectedReturn.Period.FrequencyCatalog.Name == filter.Frequency);
                }
                if (!string.IsNullOrEmpty(filter.PeriodId))
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.ExpectedReturn.PeriodId == filter.PeriodId);
                }
                /*if (!string.IsNullOrEmpty(filter.SaccoType))
                {
                    submissionsQuery = submissionsQuery.Where(rs => rs.SaccoId == filter.SaccoId);
                }*/

                var allSubmissions = await submissionsQuery.ToListAsync();

                // Group by period
                var periodGroups = allSubmissions.GroupBy(s => s.ExpectedReturn.PeriodId);

                foreach (var periodGroup in periodGroups)
                {
                    var period = periodGroup.First().ExpectedReturn.Period;
                    bool isQuarterly = period.FrequencyCatalog.Id == 5;  // QTR

                    if (isQuarterly)
                    {
                        // Quarterly: Group all forms by SACCO
                        var saccoGroups = periodGroup.GroupBy(s => s.SaccoId);

                        foreach (var saccoGroup in saccoGroups)
                        {
                            var saccoSubmissions = saccoGroup.ToList();
                            var saccoDetails = await GetSaccoDetailsAsync(saccoGroup.Key);
                            string saccoType = saccoDetails?.SaccoType ?? "0";

                            // Get all expected Q forms for this period/SACCO type
                            var expectedQForms = await _context.ExpectedReturns
                                .Where(er => er.PeriodId == period.Id && er.ReturnForm.SaccoTypeId == saccoType)
                                .Select(er => er.ReturnForm.Code)
                                .ToHashSetAsync();

                            var filedCodes = saccoSubmissions
                                .Select(s => s.ExpectedReturn.ReturnForm.Code)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

                            // Fetch CAELS rating definition to validate group (optional, for naming)
                            var ratingDef = await _context.RatingDefinations
                                .FirstOrDefaultAsync(rd => rd.RatingName.Contains("CAELS") && rd.SaccoType == saccoType);

                            // Calculate lateness statistics
                            var (hasLateSubmissions, totalDaysLate, lateFormsCount) = CalculateLatenessStats(saccoSubmissions, period);

                            results.Add(new AdminGroupedReturnDTO
                            {
                                GroupId = ratingDef?.Id ?? period.Id,  // Use rating ID or period ID
                                SaccoId = saccoGroup.Key,
                                SaccoName = saccoDetails?.SaccoName ?? "Unknown SACCO",
                                PeriodId = period.Id,
                                PeriodName = period.Name,
                                Year = period.ReportingYear.Year,
                                Frequency = period.FrequencyCatalog.Name,
                                StartDate = period.StartDate,
                                EndDate = period.EndDate,
                                SubmittedAt = saccoSubmissions.Max(s => s.SubmittedAt),
                                Status = GetGroupStatus(saccoSubmissions),
                                IsComplete = expectedQForms.SetEquals(filedCodes),  // All expected Q forms filed
                                SubmittedForms = filedCodes.Count,  // Counts all Q forms (e.g., 7)
                                Forms = await BuildFormListAsync(saccoSubmissions, expectedQForms.ToList(), period),
                                GroupType = ReturnGroupType.Grouped,
                                GroupName = $"{period.Name} {period.ReportingYear.Year} Returns - {saccoDetails?.SaccoName ?? "Unknown SACCO"}",
                                HasLateSubmissions = hasLateSubmissions,
                                TotalDaysLate = totalDaysLate,
                                LateFormsCount = lateFormsCount
                            });
                        }
                    }
                    else
                    {
                        // Non-quarterly: Standalone per form/SACCO/period
                        var saccoFormGroups = periodGroup.GroupBy(s => new { s.SaccoId, s.ExpectedReturn.ReturnFormId });

                        foreach (var saccoFormGroup in saccoFormGroups)
                        {
                            var saccoSubmissions = saccoFormGroup.ToList();
                            var saccoDetails = await GetSaccoDetailsAsync(saccoFormGroup.Key.SaccoId);
                            var form = saccoSubmissions.First().ExpectedReturn.ReturnForm;

                            // Calculate lateness statistics
                            var (hasLateSubmissions, totalDaysLate, lateFormsCount) = CalculateLatenessStats(saccoSubmissions, period);

                            results.Add(new AdminGroupedReturnDTO
                            {
                                GroupId = "standalone",
                                SaccoId = saccoFormGroup.Key.SaccoId,
                                SaccoName = saccoDetails?.SaccoName ?? "Unknown SACCO",
                                PeriodId = period.Id,
                                PeriodName = period.Name,
                                Year = period.ReportingYear.Year,
                                Frequency = period.FrequencyCatalog.Name,
                                StartDate = period.StartDate,
                                EndDate = period.EndDate,
                                SubmittedAt = saccoSubmissions.Max(s => s.SubmittedAt),
                                Status = GetGroupStatus(saccoSubmissions),
                                IsComplete = saccoSubmissions.Any(),
                                SubmittedForms = 1,  // One form per standalone
                                Forms = await BuildStandaloneFormListAsync(saccoSubmissions, period),
                                GroupType = ReturnGroupType.Standalone,
                                GroupName = $"{period.Name} {period.ReportingYear.Year} {form.FormName} - {saccoDetails?.SaccoName ?? "Unknown SACCO"}",
                                HasLateSubmissions = hasLateSubmissions,
                                TotalDaysLate = totalDaysLate,
                                LateFormsCount = lateFormsCount
                            });
                        }
                    }
                }

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
                var requiredCodes = ratingDef.RatingForms
                                      .Select(rf => rf.FormCode)
                                      .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Get all submissions for this rating definition and filter by submission date
                var submissionsQuery = _context.ReturnSubmissions
                       .Include(rs => rs.ExpectedReturn)
                           .ThenInclude(er => er.Period)
                               .ThenInclude(p => p.FrequencyCatalog)
                       .Include(rs => rs.ExpectedReturn)
                           .ThenInclude(er => er.ReturnForm)
                       .Where(rs => requiredCodes.Contains(rs.ExpectedReturn.ReturnForm.Code))
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

                    var saccoGroups = periodGroup.GroupBy(s => s.SaccoId);

                    foreach (var saccoGroup in saccoGroups)
                    {
                        var saccoSubmissions = saccoGroup.ToList();
                        var filedCodes = saccoSubmissions
                                         .Select(s => s.ExpectedReturn.ReturnForm.Code)
                                         .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        // Skip unless EVERY required form has been filed
                        if (!requiredCodes.IsSubsetOf(filedCodes))
                            continue;

                        // ── Build the DTO ────────────────────────────────────────────────
                        var saccoDetails = await GetSaccoDetailsAsync(saccoGroup.Key);
                        var Year = _context.ReportingYears.Find(period.YearId);
                        results.Add(new AdminGroupedReturnDTO
                        {
                            GroupId = ratingDef.Id,
                            SaccoId = saccoGroup.Key,
                            SaccoName = saccoDetails?.SaccoName ?? "Unknown SACCO",
                            PeriodId = period.Id,
                            PeriodName = period.Name,
                            Year = Year.Year,
                            Frequency = period.FrequencyCatalog.Name,
                            StartDate = period.StartDate,
                            EndDate = period.EndDate,
                            SubmittedAt = saccoSubmissions.Max(s => s.SubmittedAt),
                            Status = GetGroupStatus(saccoSubmissions),
                            IsComplete = true,          // guaranteed by subset check
                            SubmittedForms = filedCodes.Count,
                            Forms = await BuildFormListAsync(
                                                saccoSubmissions,
                                                requiredCodes.ToList(),
                                                period),
                            GroupType = ReturnGroupType.Grouped,
                            GroupName = $"{period.Name} {period.ReportingYear.Year} Returns - " +
                                             $"{saccoDetails?.SaccoName ?? "Unknown SACCO"}"
                        });
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
                WorkflowStateDto? workflowState = null;
                List<CommentDetails> comments = new();

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

                // Get all available versions for this period and sacco
                var allVersions = await GetAvailableVersionsAsync(periodId, saccoId);
                detailsDto.AvailableVersions = allVersions;
                /*detailsDto.HasMultipleVersions = allVersions.Count > 1;
                detailsDto.CurrentVersion = allVersions.Any() ? allVersions.First().Version : 1;
                */
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
                            detailsDto.CapitalAdequacy = await MapCapitalAdequacyWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.LiquidityStatement:
                            detailsDto.Liquidity = await MapLiquidityWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.RiskClassification:
                            detailsDto.RiskClassification = await MapRiskClassificationWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.DepositReturn:
                            detailsDto.DepositReturn = await MapDepositReturnWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.FinancialPosition:
                            detailsDto.FinancialPosition = await MapFinancialPositionWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.StatementOfComprehensiveIncome:
                            detailsDto.ComprehensiveIncome = await MapComprehensiveIncomeWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.InvestmentReturn:
                            detailsDto.InvestmentReturn = await MapInvestmentWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.Management:
                            detailsDto.Management = await MapManagementWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                    }

                    workflowState = await _workflowEngineService.GetCurrentStateAsync(null, null, submission.Id);
                    comments = await _workflowEngineService.GetComments(null, null, submission.Id);

                }
                else
                {
                    /*var ratingDef = await _context.RatingDefinations
                        .Include(rd => rd.RatingForms)
                        .FirstOrDefaultAsync(rd => rd.Id == groupId);

                    if (ratingDef == null)
                        throw new ArgumentException("Rating definition not found");*/

                    var submissions = await _context.ReturnSubmissions
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                        .Include(rs => rs.NWDTCapitalAdequacyReturns)
                        .Include(rs => rs.NWDTLiquidityReturns)
                        .Include(rs => rs.NWDTDepositReturns)
                        .Include(rs => rs.NWDTRiskClassificationReturns)
                        .Include(rs => rs.NWDTInvestmentReturns)
                        .Include(rs => rs.NWDTFinancialPositionReturns)
                        .Include(rs => rs.NWDTComprehensiveIncomeReturns)
                        .Include(rs => rs.DTCapitalAdequacyReturns)
                        .Include(rs => rs.DTLiquidityReturns)
                        .Include(rs => rs.DepositReturns)
                        .Include(rs => rs.DTRiskClassificationReturns)
                        .Include(rs => rs.DTInvestmentReturns)
                        .Include(rs => rs.DTFinancialPositionReturns)
                        .Include(rs => rs.DTComprehensiveIncomeReturns)
                        .Include(rs => rs.OtherReturns)
                        .Include(rs => rs.ManagementReturns)
                        .Where(rs => rs.ExpectedReturn.PeriodId == periodId && rs.SaccoId == saccoId)
                        .ToListAsync();

                    //var requiredFormCodes = ratingDef.RatingForms.Select(rf => rf.FormCode).ToList();
                    var submittedFormCodes = submissions.Select(s => s.ExpectedReturn.ReturnForm.Code).ToList();

                    detailsDto.SubmittedForms = submittedFormCodes.Count;
                    detailsDto.Status = GetGroupStatus(submissions);
                    detailsDto.SubmittedAt = submissions.Any() ? submissions.Max(s => s.SubmittedAt) : DateTime.MinValue;
                    detailsDto.GroupType = ReturnGroupType.Grouped;

                    // Fetch the latest consistency check result and include errors
                    var consistencyCheck = await _context.ConsistencyCheckResults
                               .Where(cc => cc.SaccoId == saccoId && cc.PeriodId == periodId)
                               .OrderByDescending(cc => cc.CheckedAt)
                               .FirstOrDefaultAsync();

                    var completenessCheck = await _context.ReturnCompleteness
                        .Where(cc => cc.SaccoId == saccoId && cc.PeriodId == periodId)
                        .OrderByDescending(cc => cc.CreatedAt)
                        .FirstOrDefaultAsync();

                    var CaelsRatings = await _context.CAELSRatings
                        .Where(cr => cr.SaccoId == saccoId && cr.PeriodId == periodId)
                        .OrderByDescending(cr => cr.CreatedAt)
                        .FirstOrDefaultAsync();



                    detailsDto.Ratings = CaelsRatings != null ? new SingleCAELSDTO
                    {
                        CapitalRating = (int)CaelsRatings.CapitalRating,
                        AssetQualityRating = (int)CaelsRatings.AssetQualityRating,
                        ManagementRating = (int)CaelsRatings.ManagementRating,
                        EarningsRating = (int)CaelsRatings.EarningsRating,
                        LiquidityRating = (int)CaelsRatings.LiquidityRating,
                        OverallRating = (int)CaelsRatings.OverallRating,
                        Average = (int)CaelsRatings.AverageRating,
                        RiskLevel = CaelsRatings.RiskLevel
                    } : new SingleCAELSDTO();

                    var isComplete = completenessCheck != null && completenessCheck.IsComplete;
                    detailsDto.IsComplete = isComplete;

                    if (consistencyCheck != null)
                    {
                        detailsDto.ConsistencyErrors = JsonSerializer.Deserialize<List<ValidationError>>(consistencyCheck.ErrorsJson) ?? new List<ValidationError>();
                    }

                    if (completenessCheck != null)
                    {
                        detailsDto.MissingForms = JsonSerializer.Deserialize<List<string>>(completenessCheck.MissingForms) ?? new List<string>();
                    }


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
                                detailsDto.DepositReturn = MapDepositReturn(submission, saccoType);
                                break;
                            case FormCategory.FinancialPosition:
                                detailsDto.FinancialPosition = MapFinancialPosition(submission, saccoType);
                                break;
                            case FormCategory.StatementOfComprehensiveIncome:
                                detailsDto.ComprehensiveIncome = MapComprehensiveIncome(submission, saccoType);
                                break;
                            case FormCategory.InvestmentReturn:
                                detailsDto.InvestmentReturn = MapInvestment(submission, saccoType);
                                break;
                            case FormCategory.Management:
                                detailsDto.Management = MapManagement(submission, saccoType);
                                break;
                            case FormCategory.Other:
                                detailsDto.Other = await MapOtherWithVersionsAsync(submission, saccoType, periodId, saccoId);
                                break;
                        }
                    }
                    workflowState = await _workflowEngineService.GetCurrentStateAsync(periodId, saccoId, null);
                    comments = await _workflowEngineService.GetComments(periodId, saccoId, null);
                }

                detailsDto.WorkflowState = workflowState;
                detailsDto.Comments = comments;
                return detailsDto;
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

        public async Task<List<ReturnVersionDTO>> GetAvailableVersionsAsync(string periodId, string saccoId, string? expectedReturnId = null)
        {
            try
            {
                var query = _context.ReturnSubmissions
                    .Include(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.ReturnForm)
                    .Where(rs => rs.ExpectedReturn.PeriodId == periodId && rs.SaccoId == saccoId);

                if (!string.IsNullOrEmpty(expectedReturnId))
                {
                    query = query.Where(rs => rs.ExpectedReturnId == expectedReturnId);
                }

                var submissions = await query
                    .OrderByDescending(rs => rs.Version)
                    .ToListAsync();

                var versions = new List<ReturnVersionDTO>();
                foreach (var submission in submissions)
                {
                    versions.Add(new ReturnVersionDTO
                    {
                        Version = submission.Version,
                        SubmissionId = submission.Id,
                        SubmittedAt = submission.SubmittedAt,
                        Status = submission.Status,
                        IsLatest = submission.IsLatest,
                        AmendsSubmissionId = submission.AmendsSubmissionId,
                        AmendedBySubmissionId = submission.AmendedBySubmissionId
                    });
                }

                return versions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available versions for period {PeriodId} and sacco {SaccoId}", periodId, saccoId);
                throw;
            }
        }

        public async Task<AdminGroupedReturnDetailsDTO> GetGroupedReturnDetailsAsync(string groupId, string periodId, string saccoId, int? version)
        {
            // If no version specified, get the latest version (default behavior)
            if (!version.HasValue)
            {
                return await GetGroupedReturnDetailsAsync(groupId, periodId, saccoId);
            }

            try
            {
                WorkflowStateDto? workflowState = null;
                List<CommentDetails> comments = new();

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
                    GroupType = ReturnGroupType.Grouped,
                    //CurrentVersion = version.Value
                };

                // Get all available versions for this period and sacco
                var allVersions = await GetAvailableVersionsAsync(periodId, saccoId);
                /* detailsDto.AvailableVersions = allVersions;
                 detailsDto.HasMultipleVersions = allVersions.Count > 1;
 */
                if (groupId == "standalone")
                {
                    var submissionsR = await _context.ReturnSubmissions
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                        .Where(rs => rs.ExpectedReturn.PeriodId == periodId && rs.SaccoId == saccoId && rs.Version == version.Value)
                        .ToListAsync();

                    if (!submissionsR.Any())
                        throw new ArgumentException($"No submissions found for version {version.Value}");

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
                            detailsDto.CapitalAdequacy = await MapCapitalAdequacyWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.LiquidityStatement:
                            detailsDto.Liquidity = await MapLiquidityWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.RiskClassification:
                            detailsDto.RiskClassification = await MapRiskClassificationWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.DepositReturn:
                            detailsDto.DepositReturn = await MapDepositReturnWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.FinancialPosition:
                            detailsDto.FinancialPosition = await MapFinancialPositionWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.StatementOfComprehensiveIncome:
                            detailsDto.ComprehensiveIncome = await MapComprehensiveIncomeWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.InvestmentReturn:
                            detailsDto.InvestmentReturn = await MapInvestmentWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                        case FormCategory.Other:
                            detailsDto.Other = await MapOtherWithVersionsAsync(submission, saccoType, periodId, saccoId);
                            break;
                    }

                    workflowState = await _workflowEngineService.GetCurrentStateAsync(null, null, submission.Id);
                    comments = await _workflowEngineService.GetComments(null, null, submission.Id);
                }
                else
                {
                    var submissions = await _context.ReturnSubmissions
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                        .Include(rs => rs.NWDTCapitalAdequacyReturns)
                        .Include(rs => rs.NWDTLiquidityReturns)
                        .Include(rs => rs.NWDTDepositReturns)
                        .Include(rs => rs.NWDTRiskClassificationReturns)
                        .Include(rs => rs.NWDTInvestmentReturns)
                        .Include(rs => rs.NWDTFinancialPositionReturns)
                        .Include(rs => rs.NWDTComprehensiveIncomeReturns)
                        .Include(rs => rs.ManagementReturns)
                        .Include(rs => rs.DTCapitalAdequacyReturns)
                        .Include(rs => rs.DTLiquidityReturns)
                        .Include(rs => rs.DepositReturns)
                        .Include(rs => rs.DTRiskClassificationReturns)
                        .Include(rs => rs.DTInvestmentReturns)
                        .Include(rs => rs.DTFinancialPositionReturns)
                        .Include(rs => rs.DTComprehensiveIncomeReturns)
                        .Include(rs => rs.OtherReturns)
                        .Where(rs => rs.ExpectedReturn.PeriodId == periodId && rs.SaccoId == saccoId && rs.Version == version.Value)
                        .ToListAsync();

                    var submittedFormCodes = submissions.Select(s => s.ExpectedReturn.ReturnForm.Code).ToList();

                    detailsDto.SubmittedForms = submittedFormCodes.Count;
                    detailsDto.Status = GetGroupStatus(submissions);
                    detailsDto.SubmittedAt = submissions.Any() ? submissions.Max(s => s.SubmittedAt) : DateTime.MinValue;
                    detailsDto.GroupType = ReturnGroupType.Grouped;

                    // Map the data for the specific version
                    foreach (var submission in submissions)
                    {
                        var form = submission.ExpectedReturn.ReturnForm;
                        switch (form.Category)
                        {
                            case FormCategory.CapitalAdequacy:
                                detailsDto.CapitalAdequacy = await MapCapitalAdequacyWithVersionsAsync(submission, saccoType, periodId, saccoId);
                                break;
                            case FormCategory.LiquidityStatement:
                                detailsDto.Liquidity = await MapLiquidityWithVersionsAsync(submission, saccoType, periodId, saccoId);
                                break;
                            case FormCategory.RiskClassification:
                                detailsDto.RiskClassification = await MapRiskClassificationWithVersionsAsync(submission, saccoType, periodId, saccoId);
                                break;
                            case FormCategory.DepositReturn:
                                detailsDto.DepositReturn = await MapDepositReturnWithVersionsAsync(submission, saccoType, periodId, saccoId);
                                break;
                            case FormCategory.FinancialPosition:
                                detailsDto.FinancialPosition = await MapFinancialPositionWithVersionsAsync(submission, saccoType, periodId, saccoId);
                                break;
                            case FormCategory.StatementOfComprehensiveIncome:
                                detailsDto.ComprehensiveIncome = await MapComprehensiveIncomeWithVersionsAsync(submission, saccoType, periodId, saccoId);
                                break;
                            case FormCategory.InvestmentReturn:
                                detailsDto.InvestmentReturn = await MapInvestmentWithVersionsAsync(submission, saccoType, periodId, saccoId);
                                break;
                            case FormCategory.Management:
                                detailsDto.InvestmentReturn = await MapManagementWithVersionsAsync(submission, saccoType, periodId, saccoId);
                                break;
                            case FormCategory.Other:
                                detailsDto.Other = await MapOtherWithVersionsAsync(submission, saccoType, periodId, saccoId);
                                break;
                        }
                    }
                    
                    workflowState = await _workflowEngineService.GetCurrentStateAsync(periodId, saccoId, null);
                    comments = await _workflowEngineService.GetComments(periodId, saccoId, null);
                }

                var ReportReadniness = await _context.ReportReadniess
                        .Where(rr => rr.SaccoId == saccoId && rr.PeriodId == periodId)
                        .OrderByDescending(rr => rr.CreatedAt)
                        .FirstOrDefaultAsync();
                
                if (ReportReadniness != null)
                {
                    detailsDto.CanReportBeViewed = ReportReadniness.IsApproved;
                }

                detailsDto.WorkflowState = workflowState;
                detailsDto.Comments = comments;
                return detailsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grouped return details for version {Version}", version);
                throw;
            }
        }

        private async Task<dynamic?> GetSaccoDetailsAsync(string saccoId)
        {
            try
            {
                var sac = saccoId;
                long LongsaccoId = long.Parse(sac);

                var sacco = await _complianceService.GetSaccoByIdAsync(LongsaccoId);
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

        /// <summary>
        /// Calculates lateness statistics for a group of submissions
        /// </summary>
        private (bool hasLateSubmissions, int totalDaysLate, int lateFormsCount) CalculateLatenessStats(
            List<ReturnSubmission> submissions, ReturnPeriods period)
        {
            var hasLateSubmissions = false;
            var totalDaysLate = 0;
            var lateFormsCount = 0;

            foreach (var submission in submissions)
            {
                var deadline = period.FilingDeadline;
                var isLate = submission.SubmittedAt > deadline;

                if (isLate)
                {
                    hasLateSubmissions = true;
                    lateFormsCount++;
                    totalDaysLate += (int)(submission.SubmittedAt - deadline).TotalDays;
                }
            }

            return (hasLateSubmissions, totalDaysLate, lateFormsCount);
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

        public static class UrlHelper
        {
            public static string BuildFullUrl(IConfiguration configuration, string relativeUrl)
            {
                var baseUrl = configuration.GetSection("GateWayConfigs:GatewayURLForDocuments").Value ?? "https://sasra-backend.sasra.go.ke";
                return new UriBuilder(baseUrl) { Path = relativeUrl.TrimStart('/') }.Uri.ToString();
            }
        }

        private object MapCapitalAdequacy(ReturnSubmission submission, string? saccoType)
        {
            if (saccoType == "0") // DT
            {
                var entity = submission.DTCapitalAdequacyReturns.FirstOrDefault();
                if (entity == null) return null;
                return new Returns.DTOs.Returns_Submission.DT.CapitalAdequacyDTO
                {
                    FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
                    SubmissionId = submission.Id,
                    ExpectedReturnId = submission.ExpectedReturnId,
                    FormCode = submission.ExpectedReturn.ReturnForm.FormName,
                    CurrentVersion = submission.Version,
                    HasMultipleVersions = false, // Will be populated by the calling method
                    AvailableVersions = new List<FormVersionDTO>(), // Will be populated by the calling method
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
                    SubmissionId = submission.Id,
                    ExpectedReturnId = submission.ExpectedReturnId,
                    FormCode = submission.ExpectedReturn.ReturnForm.FormName,
                    CurrentVersion = submission.Version,
                    HasMultipleVersions = false, // Will be populated by the calling method
                    AvailableVersions = new List<FormVersionDTO>(), // Will be populated by the calling method
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


        private async Task<List<object>?> MapOtherWithVersionsAsync(ReturnSubmission submission, string? saccoType, string periodId, string saccoId)
        {
            var results = MapOther(submission);
            foreach (var result in results)
            {
                if (result is CommonFormDTO dto)
                {
                    var versions = await GetAvailableFormVersionsAsync(periodId, saccoId, submission.ExpectedReturnId);
                    dto.AvailableVersions = versions;
                    dto.HasMultipleVersions = versions.Count > 1;
                }
            }
            
            return results;
        }

        private List<object> MapOther(ReturnSubmission submission)
        {
            List<object> otherReturns = new List<object>();
            var otherEntities = submission.OtherReturns.ToList();
            if (otherEntities == null) return null;

            foreach (var item in otherEntities)
            {
                var other = new OtherReturnDTO
                {
                    SubmissionId = submission.Id,
                    SaccoId = submission.SaccoId,
                    FileUrl = UrlHelper.BuildFullUrl(_configuration, item.FileUrl),
                    FormId = item.FormName ?? string.Empty,
                    RequiresResubmission = item.RequiresResubmission, 
                    FormName = item.FormName.Trim(),
                };
                otherReturns.Add(other);
            }
          
            return otherReturns;
        }


        private object MapInvestment(ReturnSubmission submission, string? saccoType)
        {
            if (saccoType == "0") // DT
            {
                var investmentEntity = submission.DTInvestmentReturns.FirstOrDefault();
                if (investmentEntity == null) return null;
                InvestmentReturnDTO? investment = null;
                if (investmentEntity != null)
                {
                    investment = new InvestmentReturnDTO
                    {
                        SubmissionId = submission.Id,
                        FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
                        FormId = investmentEntity.FormId ?? string.Empty,
                        RequiresResubmission = investmentEntity.RequiresResubmission,
                        CoreCapital = investmentEntity.CoreCapital,
                        TotalAssets = investmentEntity.TotalAssets,
                        TotalDeposits = investmentEntity.TotalDeposits,
                        NonEarningAssets = investmentEntity.NonEarningAssets,
                        FinancialInvestments = investmentEntity.FinancialInvestments,
                        LandAndBuildings = investmentEntity.LandAndBuildings,
                        LandBuildingsToTotalAssetsRatio = investmentEntity.LandBuildingsToTotalAssetsRatio,
                        LandBuildingsRatioExcessDeficiency = investmentEntity.LandBuildingsRatioExcessDeficiency,
                        NonEarningAssetsToTotalAssetsRatio = investmentEntity.NonEarningAssetsToTotalAssetsRatio,
                        NonEarningAssetsRatioExcessDeficiency = investmentEntity.NonEarningAssetsRatioExcessDeficiency,
                        FinancialInvestmentsToCoreCapitalRatio = investmentEntity.FinancialInvestmentsToCoreCapitalRatio,
                        FinancialInvestmentsToCoreCapitalExcessDeficiency = investmentEntity.FinancialInvestmentsToCoreCapitalExcessDeficiency,
                        FinancialInvestmentsToDepositsRatio = investmentEntity.FinancialInvestmentsToDepositsRatio,
                        FinancialInvestmentsToDepositsExcessDeficiency = investmentEntity.FinancialInvestmentsToDepositsExcessDeficiency,
                        //FilePath = investmentEntity.FilePath
                    };
                }
                return investment;
            }
            else // NWDT
            {
                NWDTInvestmentReturnDTO? nwdtInvestment = null;

                var inv = submission.NWDTInvestmentReturns.FirstOrDefault();
                if (inv == null) return null;
                if (inv != null)
                {
                    nwdtInvestment = new NWDTInvestmentReturnDTO
                    {
                        SubmissionId = submission.Id,
                        FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
                        FormId = inv.FormId,
                        RequiresResubmission = inv.RequiresResubmission,
                        CoreCapital = inv.CoreCapital,
                        TotalAssets = inv.TotalAssets,
                        TotalDeposits = inv.TotalDeposits,
                        NonEarningAssets = inv.NonEarningAssets,
                        FinancialInvestments = inv.FinancialAssets,
                        LandAndBuildings = inv.LandAndBuilding,
                        LandBuildingsToTotalAssetsRatio = inv.TotalAssets > 0
                            ? inv.LandAndBuilding / inv.TotalAssets * 100 : 0,
                        LandBuildingsRatioExcessDeficiency = inv.TotalAssets > 0
                            ? inv.LandAndBuilding / inv.TotalAssets * 100 -
                              inv.MaxLandBuildingToTotalAssetRequirement * 100 : 0,
                        NonEarningAssetsToTotalAssetsRatio = inv.TotalAssets > 0
                            ? inv.NonEarningAssets / inv.TotalAssets * 100 : 0,
                        NonEarningAssetsRatioExcessDeficiency = inv.TotalAssets > 0
                            ? inv.NonEarningAssets / inv.TotalAssets * 100 : 0,
                        FinancialInvestmentsToCoreCapitalRatio = inv.CoreCapital > 0
                            ? inv.FinancialAssets / inv.CoreCapital * 100 : 0,
                        FinancialInvestmentsToCoreCapitalExcessDeficiency = inv.CoreCapital > 0
                            ? inv.FinancialAssets / inv.CoreCapital * 100 -
                              inv.MaxFinancialInvestmentsToCoreCapital * 100 : 0,
                        FinancialInvestmentsToDepositsRatio = inv.TotalDeposits > 0
                            ? inv.FinancialAssets / inv.TotalDeposits * 100 : 0,
                        FinancialInvestmentsToDepositsExcessDeficiency = inv.TotalDeposits > 0
                            ? inv.FinancialAssets / inv.TotalDeposits * 100 -
                              inv.MaxEquityInvestmentsToTotalDeposits * 100 : 0,
                        //FilePath = inv.FilePath
                    };
                }

                return nwdtInvestment;
            }
        }


        private object MapManagement(ReturnSubmission submission, string? saccoType)
        {

            var mgr = submission.ManagementReturns.FirstOrDefault();
            if (mgr == null) return null;
            ManagementReturnDTO? management = null;
            if (mgr != null)
            {
                management = new ManagementReturnDTO
                {
                    SubmissionId = mgr.ReturnSubmissionId,
                    SaccoCsNumber = mgr.SaccoCsNumber,
                    GovernanceStructureScore = mgr.GorvenanceStructureScore,
                    GovernanceStructureWeight = mgr.GorvenanceStructureWeight,
                    GovernanceStructureWeightedScore = mgr.GorvenanceStructureWeightedScore,
                    InternalControlsScore = mgr.InternalControlsScore,
                    InternalControlsWeight = mgr.InternalControlsWeight,
                    InternalControlsWeightedScore = mgr.InternalControlsWeightedScore,
                    ComplianceWithLawsScore = mgr.ComplianceWithLawsAndRegulationsScore,
                    ComplianceWithLawsWeight = mgr.ComplianceWithLawsAndRegulationsWeight,
                    ComplianceWithLawsWeightedScore = mgr.ComplianceWithLawsAndRegulationsWeightedScore,
                    MemberProtectionScore = mgr.MemberProtectionScore,
                    MemberProtectionWeight = mgr.MemberProtectionWeight,
                    MemberProtectionWeightedScore = mgr.MemberProtectionWeightedScore,
                    AdequacyOfMISScore = mgr.AdequacyOfMISScore,
                    AdequacyOfMISWeight = mgr.AdequacyOfMISWeight,
                    AdequacyOfMISWeightedScore = mgr.AdequacyOfMISWeightedScore,
                    OverallRiskProfileScore = mgr.OverallRiskProfileScore,
                    OverallRiskProfileWeight = mgr.OverallRiskProfileWeight,
                    OverallRiskProfileWeightedScore = mgr.OverallRiskProfileWeightedScore,
                    MRating = mgr.MRating
                };

            }
            return management;

        }



        private object MapLiquidity(ReturnSubmission submission, string? saccoType)
        {
            if (saccoType == "0") // DT
            {
                var liquidityEntity = submission.DTLiquidityReturns.FirstOrDefault();
                if (liquidityEntity == null) return null;
                return new DTOs.Returns_Submission.DT.LiquidityStatementDTO
                {
                    SubmissionId = submission.Id,
                    FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
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
                    SubmissionId = submission.Id,
                    FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
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
                var riskClassifications = new DTOs.Returns_Submission.DT.RiskClassificationDTO();
                var entities = submission.DTRiskClassificationReturns.ToList();
                if (entities == null || entities.Count <= 0) return null;

                var firstEntity = entities.First();
                riskClassifications.FormId = firstEntity.FormId ?? string.Empty;
                riskClassifications.SubmissionId = submission.Id;
                riskClassifications.RequiresResubmission = firstEntity.RequiresResubmission;
                riskClassifications.FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl);

                foreach (var rc in entities)
                {
                    riskClassifications.RiskClassificationData.Add(new RiskClassificationData
                    {

                        LoanType = rc.LoanType,
                        Classification = rc.Classification,
                        NumberOfAccounts = rc.NumberOfAccounts,
                        OutstandingLoanPortfolio = rc.OutstandingLoanPortfolio,
                        RequiredProvision = rc.RequiredProvision,
                        RequiredProvisionAmount = rc.RequiredProvisionAmount,
                    });
                }

                return riskClassifications;

            }
            else // NWDT
            {
                var risks = submission.NWDTRiskClassificationReturns.ToList();
                NWDTRiskClassificationDTO? nwdtRiskClassification = null;

                nwdtRiskClassification = new NWDTRiskClassificationDTO();

                var firstItem = risks.First();
                nwdtRiskClassification.FormId = firstItem.FormId;
                nwdtRiskClassification.RequiresResubmission = firstItem.RequiresResubmission;
                nwdtRiskClassification.FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl);
                nwdtRiskClassification.FormId = submission.Id;

                nwdtRiskClassification.NWDTRiskClassificationData = risks.Select(rc => new NWDTRiskClassificationData
                {
                    LoanType = rc.LoanType,
                    Classification = rc.Classification,
                    NumberOfAccounts = rc.NumberOfAccounts,
                    OutstandingLoanPortfolio = rc.OutstandingLoanPortfolio,
                    RequiredProvision = rc.RequiredProvision,
                    RequiredProvisionAmount = rc.RequiredProvisionAmount,
                    //FilePath = rc.FilePath
                }).ToList();

                return nwdtRiskClassification;
            }
        }

        private object MapDepositReturn(ReturnSubmission submission, string? saccoType)
        {
            if (saccoType == "0") // DT
            {
                DepositReturnDto? depositReturn = null;

                var depositEntities = submission.DepositReturns.ToList();
                if (depositEntities == null || !depositEntities.Any())
                {
                    return null;
                }

                depositReturn = new DepositReturnDto
                {
                    FormId = depositEntities.FirstOrDefault()?.FormId ?? string.Empty,
                    SubmissionId = submission.Id,
                    RequiresResubmission = depositEntities.FirstOrDefault()?.RequiresResubmission ?? false,
                    FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
                };

                foreach (var dr in depositEntities)
                {
                    depositReturn.DepositReturnData.Add(new DepositReturnData
                    {
                        RangeName = dr.RangeName,
                        DepositType = dr.DepositType,
                        NumberOfAccounts = dr.NumberOfAccounts,
                        Amount = dr.AmountInKshs000
                    });
                }

                return depositReturn;

            }
            else // NWDT
            {
                var deposits = submission.NWDTDepositReturns.ToList();
                if (deposits.Count <= 0) return null;
                NWDTDepositReturnDto? nwdtDepositReturn = null;

                nwdtDepositReturn = new NWDTDepositReturnDto
                {
                    FormId = deposits[0].FormId ?? string.Empty,
                    RequiresResubmission = deposits[0].RequiresResubmission,
                    SubmissionId = submission.Id,
                    FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
                    DepositReturnData = deposits.Select(dr => new NWDTDepositReturnData
                    {
                        RangeName = dr.RangeName ?? string.Empty,
                        DepositType = dr.DepositType ?? string.Empty,
                        NumberOfAccounts = dr.NumberOfAccounts,
                        Amount = dr.AmountInKshs000,
                    }).ToList()
                };

                return nwdtDepositReturn;
            }
        }

        private object MapFinancialPosition(ReturnSubmission submission, string? saccoType)
        {
            if (saccoType == "0") // DT
            {
                var balanceEntity = submission.DTFinancialPositionReturns.FirstOrDefault();
                DTOs.Returns_Submission.Returns_Submission.DT.FinancialPositionDTO? financialPosition = null;

                if (balanceEntity != null)
                {
                    financialPosition = new Returns.DTOs.Returns_Submission.Returns_Submission.DT.FinancialPositionDTO
                    {
                        SubmissionId = submission.Id,
                        FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
                        FormId = balanceEntity.FormId ?? string.Empty,
                        RequiresResubmission = balanceEntity.RequiresResubmission,
                        CashInHand = balanceEntity.CashInHand,
                        CashAtBank = balanceEntity.CashAtBank,
                        PrepaymentsAndSundryReceivables = balanceEntity.PrepaymentsAndSundryReceivables,
                        GovernmentSecurities = balanceEntity.GovernmentSecurities,
                        OtherSecurities = balanceEntity.OtherSecurities,
                        BalancesWithOtherSaccos = balanceEntity.BalancesWithOtherSaccos,
                        InvestmentsInCompanies = balanceEntity.InvestmentsInCompanies,
                        GrossLoanPortfolio = balanceEntity.GrossLoanPortfolio,
                        AllowanceForLoanLoss = balanceEntity.AllowanceForLoanLoss,
                        TaxRecoverable = balanceEntity.TaxRecoverable,
                        DeferredTaxAssets = balanceEntity.DeferredTaxAssets,
                        RetirementBenefitAssets = balanceEntity.RetirementBenefitAssets,
                        InvestmentProperties = balanceEntity.InvestmentProperties,
                        PropertyAndEquipment = balanceEntity.PropertyAndEquipment,
                        PrepaidLeaseRentals = balanceEntity.PrepaidLeaseRentals,
                        IntangibleAssets = balanceEntity.IntangibleAssets,
                        OtherAssets = balanceEntity.OtherAssets,
                        SavingsDeposits = balanceEntity.SavingsDeposits,
                        ShortTermDeposits = balanceEntity.ShortTermDeposits,
                        NonWithdrawableDeposits = balanceEntity.NonWithdrawableDeposits,
                        TaxPayable = balanceEntity.TaxPayable,
                        DividendsPayable = balanceEntity.DividendsPayable,
                        DeferredTaxLiability = balanceEntity.DeferredTaxLiability,
                        RetirementBenefitsLiability = balanceEntity.RetirementBenefitsLiability,
                        OtherLiabilities = balanceEntity.OtherLiabilities,
                        ExternalBorrowings = balanceEntity.ExternalBorrowings,
                        ShareCapital = balanceEntity.ShareCapital,
                        CapitalGrants = balanceEntity.CapitalGrants,
                        PriorYearsRetainedEarnings = balanceEntity.PriorYearsRetainedEarnings,
                        CurrentYearSurplus = balanceEntity.CurrentYearSurplus,
                        StatutoryReserve = balanceEntity.StatutoryReserve,
                    };
                    return financialPosition;
                }
                return null;
            }
            else // NWDT
            {
                var entity = submission.NWDTFinancialPositionReturns.FirstOrDefault();
                NWDTFinancialPositionDTO? nwdtFinancialPosition = null;

                if (entity != null)
                {
                    nwdtFinancialPosition = new NWDTFinancialPositionDTO
                    {
                        SubmissionId = submission.Id,
                        FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
                        FormId = entity.FormId ?? string.Empty,
                        RequiresResubmission = entity.RequiresResubmission,
                        CashInHand = entity.CashInHand,
                        CashAtBank = entity.CashAtBank,
                        TotalCashAndCashEquivalent = entity.StoredCashAndCashEquivalent,
                        PrepaymentsAndSundryReceivables = entity.PrepaymentsAndSundryReceivables,
                        GovernmentSecurities = entity.GovernmentSecurities,
                        InvestmentsInCompanies = entity.InvestmentInCompanies,
                        TotalFinancialInvestments = entity.StoredFinancialInvestments,
                        GrossLoanPortfolio = entity.GrossLoanPortfolio,
                        AllowanceForLoanLoss = entity.AllowanceForLoanLoss,
                        NetLoanPortfolio = entity.StoredNetLoanPortfolio,
                        TaxRecoverable = entity.TaxRecoverable,
                        DeferredTaxAssets = entity.DeferredTaxAssets,
                        RetirementBenefitAssets = entity.RetirementBenefitAssets,
                        TotalAccountsReceivables = entity.StoredAccountsReceivables,
                        InvestmentProperties = entity.InvestmentProperties,
                        PropertyAndEquipment = entity.PropertyAndEquipment,
                        PrepaidLeaseRentals = entity.PrepaidLeaseRentals,
                        IntangibleAssets = entity.IntangibleAssets,
                        OtherAssets = entity.OtherAssets,
                        TotalPropertyAndEquipment = entity.StoredPropertyEquipmentOtherAssets,
                        TotalAssets = entity.StoredTotalAssets,
                        NonWithdrawableDeposits = entity.NonWithdrawableDeposits,
                        TotalDepositLiabilities = entity.StoredTotalDepositLiabilities,
                        TaxPayable = entity.TaxPayable,
                        DividendsPayable = entity.DividendsPayable,
                        DeferredTaxLiability = entity.DeferredTaxLiability,
                        RetirementBenefitsLiability = entity.RetirementBenefitsLiability,
                        OtherLiabilities = entity.OtherLiabilities,
                        ExternalBorrowings = entity.ExternalBorrowings,
                        TotalAccountsPayable = entity.StoredAccountsPayableOtherLiabilities,
                        TotalLiabilities = entity.StoredTotalLiabilities,
                        ShareCapital = entity.ShareCapital,
                        CapitalGrants = entity.CapitalGrants,
                        PriorYearsRetainedEarnings = entity.PriorYearsRetainedEarnings,
                        CurrentYearSurplus = entity.CurrentYearSurplus,
                        TotalRetainedEarnings = entity.StoredRetainedEarnings,
                        StatutoryReserve = entity.StatutoryReserve,
                        OtherReserves = entity.OtherReserves,
                        RevaluationReserves = entity.RevaluationReserves,
                        ProposedDividends = entity.ProposedDividends,
                        AdjustmentToEquity = entity.AdjustmentToEquity,
                        TotalOtherEquityAccounts = entity.StoredOtherEquityAccounts,
                        TotalEquity = entity.StoredTotalEquity
                    };
                    return nwdtFinancialPosition;
                }
                return null;
            }
        }

        private object MapComprehensiveIncome(ReturnSubmission submission, string? saccoType)
        {
            if (saccoType == "0") // DT
            {
                var incomeEntity = submission.DTComprehensiveIncomeReturns.FirstOrDefault();
                ComprehesiveIncomeStatementDTO? incomeStatement = null;

                if (incomeEntity != null)
                {
                    incomeStatement = new ComprehesiveIncomeStatementDTO
                    {
                        SubmissionId = submission.Id,
                        FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
                        FormId = incomeEntity.FormId ?? string.Empty,
                        RequiresResubmission = incomeEntity.RequiresResubmission,
                        InterestOnLoanPortfolio = incomeEntity.InterestOnLoanPortfolio,
                        FeesAndCommissionOnLoanPortfolio = incomeEntity.FeesAndCommissionOnLoanPortfolio,
                        GovernmentSecurities = incomeEntity.GovernmentSecurities,
                        DepositsWithBanks = incomeEntity.DepositsWithBanks,
                        OtherInvestments = incomeEntity.OtherInvestments,
                        OtherOperatingIncome = incomeEntity.OtherOperatingIncome,
                        InterestExpenseOnDeposits = incomeEntity.InterestExpenseOnDeposits,
                        CostOfExternalBorrowings = incomeEntity.CostOfExternalBorrowings,
                        DividendExpenses = incomeEntity.DividendExpenses,
                        OtherFinancialExpense = incomeEntity.OtherFinancialExpense,
                        FeesAndCommissionExpense = incomeEntity.FeesAndCommissionExpense,
                        OtherExpense = incomeEntity.OtherExpense,
                        ProvisionForLoanLosses = incomeEntity.ProvisionForLoanLosses,
                        ValueOfLoansRecovered = incomeEntity.ValueOfLoansRecovered,
                        PersonnelExpenses = incomeEntity.PersonnelExpenses,
                        GovernanceExpenses = incomeEntity.GovernanceExpenses,
                        MarketingExpenses = incomeEntity.MarketingExpenses,
                        DepreciationAndAmortization = incomeEntity.DepreciationAndAmortization,
                        AdministrativeExpenses = incomeEntity.AdministrativeExpenses,
                        NonOperatingIncome = incomeEntity.NonOperatingIncome,
                        NonOperatingExpense = incomeEntity.NonOperatingExpense,
                        Taxes = incomeEntity.Taxes,
                        Donations = incomeEntity.Donations,
                    };

                    return incomeStatement;
                }
                return null;
            }
            else // NWDT
            {
                var inc = submission.NWDTComprehensiveIncomeReturns.FirstOrDefault(); // Assuming NWDT uses the same DTO for Comprehensive Income
                NWDTComprehesiveIncomeStatementDTO? nwdtIncomeStatement = null;

                if (inc != null)
                {
                    nwdtIncomeStatement = new NWDTComprehesiveIncomeStatementDTO
                    {
                        FileUrl = UrlHelper.BuildFullUrl(_configuration, submission.FileUrl),
                        SubmissionId = submission.Id,
                        FormId = inc.FormId,
                        RequiresResubmission = inc.RequiresResubmission,
                        InterestOnLoanPortfolio = inc.InterestOnLoanPortfolio,
                        FeesAndCommissionOnLoanPortfolio = inc.FeesCommissionOnLoanPortfolio,
                        TotalFinancialIncomeFromLoans = inc.FinancialIncomeFromLoansPortfolio,
                        GovernmentSecurities = inc.GovernmentSecuritiesIncome,
                        DepositsWithBanks = inc.PlacementInBanksIncome,
                        OtherInvestments =
                            inc.CommercialPapersIncome +
                            inc.CollectiveInvestmentSchemesIncome +
                            inc.DerivativesIncome +
                            inc.EquityInvestmentsIncome +
                            inc.InvestmentInCompaniesIncome,
                        TotalFinancialIncomeFromInvestments = inc.FinancialIncomeFromInvestments,
                        TotalFinancialIncome = inc.FinancialIncome,
                        InterestExpenseOnDeposits = inc.InterestExpenseOnDeposits,
                        CostOfExternalBorrowings = inc.CostOfExternalBorrowings,
                        DividendExpenses = inc.DividendExpenses,
                        OtherFinancialExpense = inc.OtherFinancialExpense,
                        FeesAndCommissionExpense = inc.FeesCommissionExpense,
                        OtherExpense = inc.OtherExpense,
                        TotalFinancialExpense = inc.FinancialExpense,
                        NetFinancialIncome = inc.NetFinancialIncome,
                        ProvisionForLoanLosses = inc.ProvisionForLoanLosses,
                        ValueOfLoansRecovered = inc.ValueOfLoansRecovered,
                        NetAllowanceForLoanLoss = inc.AllowanceForLoanLoss,
                        PersonnelExpenses = inc.PersonnelExpenses,
                        GovernanceExpenses = inc.GovernanceExpenses,
                        MarketingExpenses = inc.MarketingExpenses,
                        DepreciationAndAmortization = inc.DepreciationAmortizationCharges,
                        AdministrativeExpenses = inc.AdministrativeExpenses,
                        TotalOperatingExpenses = inc.OperatingExpenses,
                        NetOperatingIncome = inc.NetOperatingIncome,
                        NonOperatingIncome = inc.NonOperatingIncome,
                        NonOperatingExpense = inc.NonOperatingExpense,
                        NetNonOperatingIncome = inc.NetNonOperatingIncome,
                        Taxes = inc.Taxes,
                        NetIncomeBeforeTaxes = inc.NetIncomeBeforeTaxes,
                        NetIncomeAfterTaxes = inc.NetIncomeAfterTaxesBeforeDonations,
                        Donations = inc.Donations,
                        NetIncomeAfterTaxesAndDonations = inc.NetIncomeAfterTaxesAndDonations,
                    };
                    return nwdtIncomeStatement;
                }
                return null;
            }
        }

        // Async mapping methods with version information
        private async Task<object?> MapCapitalAdequacyWithVersionsAsync(ReturnSubmission submission, string? saccoType, string periodId, string saccoId)
        {
            var result = MapCapitalAdequacy(submission, saccoType);
            if (result is CommonFormDTO dto)
            {
                var versions = await GetAvailableFormVersionsAsync(periodId, saccoId, submission.ExpectedReturnId);
                dto.AvailableVersions = versions;
                dto.HasMultipleVersions = versions.Count > 1;
            }
            return result;
        }

        private async Task<object?> MapLiquidityWithVersionsAsync(ReturnSubmission submission, string? saccoType, string periodId, string saccoId)
        {
            var result = MapLiquidity(submission, saccoType);
            if (result is CommonFormDTO dto)
            {
                var versions = await GetAvailableFormVersionsAsync(periodId, saccoId, submission.ExpectedReturnId);
                dto.AvailableVersions = versions;
                dto.HasMultipleVersions = versions.Count > 1;
            }
            return result;
        }

        private async Task<object?> MapRiskClassificationWithVersionsAsync(ReturnSubmission submission, string? saccoType, string periodId, string saccoId)
        {
            var result = MapRiskClassification(submission, saccoType);
            if (result is CommonFormDTO dto)
            {
                var versions = await GetAvailableFormVersionsAsync(periodId, saccoId, submission.ExpectedReturnId);
                dto.AvailableVersions = versions;
                dto.HasMultipleVersions = versions.Count > 1;
            }
            return result;
        }

        private async Task<object?> MapDepositReturnWithVersionsAsync(ReturnSubmission submission, string? saccoType, string periodId, string saccoId)
        {
            var result = MapDepositReturn(submission, saccoType);
            if (result is CommonFormDTO dto)
            {
                var versions = await GetAvailableFormVersionsAsync(periodId, saccoId, submission.ExpectedReturnId);
                dto.AvailableVersions = versions;
                dto.HasMultipleVersions = versions.Count > 1;
            }
            return result;
        }

        private async Task<object?> MapFinancialPositionWithVersionsAsync(ReturnSubmission submission, string? saccoType, string periodId, string saccoId)
        {
            var result = MapFinancialPosition(submission, saccoType);
            if (result is CommonFormDTO dto)
            {
                var versions = await GetAvailableFormVersionsAsync(periodId, saccoId, submission.ExpectedReturnId);
                dto.AvailableVersions = versions;
                dto.HasMultipleVersions = versions.Count > 1;
            }
            return result;
        }

        private async Task<object?> MapComprehensiveIncomeWithVersionsAsync(ReturnSubmission submission, string? saccoType, string periodId, string saccoId)
        {
            var result = MapComprehensiveIncome(submission, saccoType);
            if (result is CommonFormDTO dto)
            {
                var versions = await GetAvailableFormVersionsAsync(periodId, saccoId, submission.ExpectedReturnId);
                dto.AvailableVersions = versions;
                dto.HasMultipleVersions = versions.Count > 1;
            }
            return result;
        }

        private async Task<object?> MapInvestmentWithVersionsAsync(ReturnSubmission submission, string? saccoType, string periodId, string saccoId)
        {
            var result = MapInvestment(submission, saccoType);
            if (result is CommonFormDTO dto)
            {
                var versions = await GetAvailableFormVersionsAsync(periodId, saccoId, submission.ExpectedReturnId);
                dto.AvailableVersions = versions;
                dto.HasMultipleVersions = versions.Count > 1;
            }
            return result;
        }

        private async Task<object?> MapManagementWithVersionsAsync(ReturnSubmission submission, string? saccoType, string periodId, string saccoId)
        {
            var result = MapManagement(submission, saccoType);
            if (result is CommonFormDTO dto)
            {
                var versions = await GetAvailableFormVersionsAsync(periodId, saccoId, submission.ExpectedReturnId);
                dto.AvailableVersions = versions;
                dto.HasMultipleVersions = versions.Count > 1;
            }
            return result;
        }

        // Individual form versioning methods
        public async Task<List<FormVersionDTO>> GetAvailableFormVersionsAsync(string periodId, string saccoId, string expectedReturnId)
        {
            try
            {
                var query = _context.ReturnSubmissions
                    .Include(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.ReturnForm)
                    .Where(rs => rs.ExpectedReturn.PeriodId == periodId &&
                                rs.SaccoId == saccoId &&
                                rs.ExpectedReturnId == expectedReturnId);

                var submissions = await query
                    .OrderByDescending(rs => rs.Version)
                    .ToListAsync();

                var versions = new List<FormVersionDTO>();
                foreach (var submission in submissions)
                {
                    versions.Add(new FormVersionDTO
                    {
                        Version = submission.Version,
                        SubmissionId = submission.Id,
                        SubmittedAt = submission.SubmittedAt,
                        Status = submission.Status,
                        IsLatest = submission.IsLatest,
                        AmendsSubmissionId = submission.AmendsSubmissionId,
                        AmendedBySubmissionId = submission.AmendedBySubmissionId,
                        ExpectedReturnId = submission.ExpectedReturnId,
                        FormCode = submission.ExpectedReturn.ReturnForm.FormName
                    });
                }
                return versions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available form versions for period {PeriodId}, sacco {SaccoId}, and expected return {ExpectedReturnId}", periodId, saccoId, expectedReturnId);
                throw;
            }
        }

        public async Task<object?> GetFormDataByVersionAsync(string submissionId, string expectedReturnId, string? saccoType = null)
        {
            try
            {
                var submission = await _context.ReturnSubmissions
                    .Include(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.ReturnForm)
                    .FirstOrDefaultAsync(rs => rs.Id == submissionId && rs.ExpectedReturnId == expectedReturnId);

                if (submission == null)
                {
                    return null;
                }

                // Determine sacco type if not provided
                if (string.IsNullOrEmpty(saccoType))
                {
                    saccoType = submission.ExpectedReturn.ReturnForm.FormName.StartsWith("DT") ? "0" : "1";
                }

                // Map the form data based on the expected return type
                var formCode = submission.ExpectedReturn.ReturnForm.FormName;

                switch (formCode)
                {
                    case "DT_CAPITAL_ADEQUACY":
                    case "NWDT_CAPITAL_ADEQUACY":
                        return MapCapitalAdequacy(submission, saccoType);
                    case "DT_LIQUIDITY":
                    case "NWDT_LIQUIDITY":
                        return MapLiquidity(submission, saccoType);
                    case "DT_RISK_CLASSIFICATION":
                    case "NWDT_RISK_CLASSIFICATION":
                        return MapRiskClassification(submission, saccoType);
                    case "DT_DEPOSIT_RETURN":
                    case "NWDT_DEPOSIT_RETURN":
                        return MapDepositReturn(submission, saccoType);
                    case "DT_FINANCIAL_POSITION":
                    case "NWDT_FINANCIAL_POSITION":
                        return MapFinancialPosition(submission, saccoType);
                    case "DT_COMPREHENSIVE_INCOME":
                    case "NWDT_COMPREHENSIVE_INCOME":
                        return MapComprehensiveIncome(submission, saccoType);
                    case "DT_INVESTMENT":
                    case "NWDT_INVESTMENT":
                        return MapInvestment(submission, saccoType);
                    default:
                        _logger.LogWarning("Unknown form code: {FormCode}", formCode);
                        return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting form data by version for submission {SubmissionId} and expected return {ExpectedReturnId}", submissionId, expectedReturnId);
                throw;
            }
        }
    }
}