using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns_Submission;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class AdminReturnService : IAdminReturnService
    {
        private readonly ReturnsDbContext _context;
        private readonly ILogger<AdminReturnService> _logger;

        public AdminReturnService(ReturnsDbContext context, ILogger<AdminReturnService> logger)
        {
            _context = context;
            _logger = logger;
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
            // Start with rating definitions
            var query = _context.RatingDefinations
                .Include(rd => rd.RatingForms)
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
                // Get all periods that match the filter
                var periodsQuery = _context.ReturnPeriods
                    .Include(p => p.FrequencyCatalog)
                    .Include(p => p.ReportingYear)
                    .AsQueryable();

                if (filter.Year.HasValue)
                {
                    periodsQuery = periodsQuery.Where(p => p.ReportingYear.Year == filter.Year.Value);
                }

                if (filter.Month.HasValue)
                {
                    periodsQuery = periodsQuery.Where(p => p.StartDate.Month == filter.Month.Value);
                }

                if (!string.IsNullOrEmpty(filter.Frequency))
                {
                    periodsQuery = periodsQuery.Where(p => p.FrequencyCatalog.Name == filter.Frequency);
                }

                if (!string.IsNullOrEmpty(filter.PeriodId))
                {
                    periodsQuery = periodsQuery.Where(p => p.Id == filter.PeriodId);
                }

                var periods = await periodsQuery.ToListAsync();

                foreach (var period in periods)
                {
                    // Get all submissions for this period and rating definition
                    var submissions = await _context.ReturnSubmissions
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                        .Where(rs => rs.ExpectedReturn.PeriodId == period.Id)
                        .ToListAsync();

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
                            RatingName = ratingDef.RatingName,
                            Description = ratingDef.Description,
                            SaccoType = ratingDef.SaccoType,
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
                            TotalRequiredForms = requiredFormCodes.Count,
                            SubmittedForms = submittedFormCodes.Count,
                            Forms = await BuildFormListAsync(saccoSubmissions, requiredFormCodes, period),
                            GroupType = ReturnGroupType.Grouped
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

            // Get periods that match the filter
            var periodsQuery = _context.ReturnPeriods
                .Include(p => p.FrequencyCatalog)
                .Include(p => p.ReportingYear)
                .AsQueryable();

            if (filter.Year.HasValue)
            {
                periodsQuery = periodsQuery.Where(p => p.ReportingYear.Year == filter.Year.Value);
            }

            if (filter.Month.HasValue)
            {
                periodsQuery = periodsQuery.Where(p => p.StartDate.Month == filter.Month.Value);
            }

            if (!string.IsNullOrEmpty(filter.Frequency))
            {
                periodsQuery = periodsQuery.Where(p => p.FrequencyCatalog.Name == filter.Frequency);
            }

            if (!string.IsNullOrEmpty(filter.PeriodId))
            {
                periodsQuery = periodsQuery.Where(p => p.Id == filter.PeriodId);
            }

            var periods = await periodsQuery.ToListAsync();

            foreach (var period in periods)
            {
                foreach (var form in standaloneForms)
                {
                    // Get submissions for this specific form and period
                    var submissions = await _context.ReturnSubmissions
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                        .Where(rs => rs.ExpectedReturn.PeriodId == period.Id && 
                                    rs.ExpectedReturn.ReturnFormId == form.Id)
                        .ToListAsync();

                    // Group by SACCO
                    var saccoGroups = submissions.GroupBy(s => s.SaccoId);

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
                            RatingName = form.FormName,
                            Description = $"Standalone return: {form.FormName}",
                            SaccoType = form.SaccoTypeId,
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
                            TotalRequiredForms = 1,
                            SubmittedForms = saccoSubmissions.Any() ? 1 : 0,
                            Forms = await BuildStandaloneFormListAsync(saccoSubmissions, period),
                            GroupType = ReturnGroupType.Standalone
                        };

                        results.Add(standaloneReturn);
                    }
                }
            }

            return results;
        }

        public async Task<AdminGroupedReturnDTO> GetGroupedReturnDetailsAsync(string groupId, string periodId, string saccoId)
        {
            try
            {
                var period = await _context.ReturnPeriods
                    .Include(p => p.FrequencyCatalog)
                    .Include(p => p.ReportingYear)
                    .FirstOrDefaultAsync(p => p.Id == periodId);

                if (period == null)
                    throw new ArgumentException("Period not found");

                // Handle standalone returns
                if (groupId == "standalone")
                {
                    var submissions = await _context.ReturnSubmissions
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.Period)
                        .Include(rs => rs.ExpectedReturn)
                        .ThenInclude(er => er.ReturnForm)
                        .Where(rs => rs.ExpectedReturn.PeriodId == periodId && rs.SaccoId == saccoId)
                        .ToListAsync();

                    if (!submissions.Any())
                        throw new ArgumentException("No submissions found for this SACCO and period");

                    var saccoDetails = await GetSaccoDetailsAsync(saccoId);
                    var submission = submissions.First();

                    return new AdminGroupedReturnDTO
                    {
                        GroupId = "standalone",
                        RatingName = submission.ExpectedReturn.ReturnForm.FormName,
                        Description = $"Standalone return: {submission.ExpectedReturn.ReturnForm.FormName}",
                        SaccoType = submission.ExpectedReturn.ReturnForm.SaccoTypeId,
                        SaccoId = saccoId,
                        SaccoName = saccoDetails?.SaccoName ?? "Unknown SACCO",
                        PeriodId = period.Id,
                        PeriodName = period.Name,
                        Year = period.ReportingYear.Year,
                        Frequency = period.FrequencyCatalog.Name,
                        StartDate = period.StartDate,
                        EndDate = period.EndDate,
                        SubmittedAt = submission.SubmittedAt,
                        Status = GetGroupStatus(submissions),
                        IsComplete = submissions.Any(),
                        TotalRequiredForms = 1,
                        SubmittedForms = submissions.Any() ? 1 : 0,
                        Forms = await BuildStandaloneFormListAsync(submissions, period),
                        GroupType = ReturnGroupType.Standalone
                    };
                }

                // Handle grouped returns
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

                var saccoDetails = await GetSaccoDetailsAsync(saccoId);
                var requiredFormCodes = ratingDef.RatingForms.Select(rf => rf.FormCode).ToList();
                var submittedFormCodes = submissions.Select(s => s.ExpectedReturn.ReturnForm.Code).ToList();
                var isComplete = requiredFormCodes.All(required => submittedFormCodes.Contains(required));

                return new AdminGroupedReturnDTO
                {
                    GroupId = ratingDef.Id,
                    RatingName = ratingDef.RatingName,
                    Description = ratingDef.Description,
                    SaccoType = ratingDef.SaccoType,
                    SaccoId = saccoId,
                    SaccoName = saccoDetails?.SaccoName ?? "Unknown SACCO",
                    PeriodId = period.Id,
                    PeriodName = period.Name,
                    Year = period.ReportingYear.Year,
                    Frequency = period.FrequencyCatalog.Name,
                    StartDate = period.StartDate,
                    EndDate = period.EndDate,
                    SubmittedAt = submissions.Any() ? submissions.Max(s => s.SubmittedAt) : DateTime.MinValue,
                    Status = GetGroupStatus(submissions),
                    IsComplete = isComplete,
                    TotalRequiredForms = requiredFormCodes.Count,
                    SubmittedForms = submittedFormCodes.Count,
                    Forms = await BuildFormListAsync(submissions, requiredFormCodes, period),
                    GroupType = ReturnGroupType.Grouped
                };
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
                // This would typically call your compliance service
                // For now, return a simple object
                return new { SaccoName = $"SACCO {saccoId}" };
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

                if (submission.SubmittedAt.HasValue)
                {
                    var deadline = period.FilingDeadline;
                    isLate = submission.SubmittedAt.Value > deadline;
                    daysLate = isLate ? (int)(submission.SubmittedAt.Value - deadline).TotalDays : 0;
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
    }
}