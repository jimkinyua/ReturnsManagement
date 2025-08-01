using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System;
using static Returns.Helpers.Constants;
using Microsoft.Extensions.Caching.Memory;
using static Returns.Helpers.TokenHelper;
using Hangfire;
using System.Text.Json;
using Returns.DTOs.Forms;

namespace Returns.Helpers
{
    public class ReturnSubmissionService : IReturnSubmissionService
    {
        private readonly ReturnsDbContext _context;
        private readonly IExcelParser _excelParser;
        private readonly ILogger<ReturnSubmissionService> _logger;
        private readonly IMemoryCache _cache;
        private const string SubmissionStatusCacheKey = "SubmissionStatus_";
        private readonly IComplianceService complianceService;
        private readonly IEmailService _emailService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IWorkflowEngineService _workflowService;
        private readonly ICamelsAnalysisService _camelsAnalysisService;
        private readonly IConsistencyCheckService _consistencyCheckService;



        public ReturnSubmissionService(
            ReturnsDbContext context,
            IComplianceService complianceService,
            IEmailService emailService,
            IBackgroundJobClient backgroundJobClient,
            IExcelParser excelParser,
            IConsistencyCheckService consistencyCheckService,
            IWorkflowEngineService workflowService,
            ICamelsAnalysisService camelsAnalysisService,
            ILogger<ReturnSubmissionService> logger,
            IMemoryCache cache)
        {
            _context = context;
            this.complianceService = complianceService;
            _emailService = emailService;
            _excelParser = excelParser;
            _logger = logger;
            _cache = cache;
            _consistencyCheckService = consistencyCheckService;
            _backgroundJobClient = backgroundJobClient;
            _workflowService = workflowService;
            _camelsAnalysisService = camelsAnalysisService;
            _logger = logger;
            _cache = cache;

        }

        public async Task<IList<SubmissionResultDto>> UploadDraftAsync(NewReturnDTO dto, LoggedInEntity loggedInSacco, Boolean IsAmendment=false)
        {
            var results = new List<SubmissionResultDto>();

            foreach (var item in dto.FormUploads)
            {
                var res = new SubmissionResultDto();
                res.FormFileName = item.formFile?.FileName ?? "Unknown";
                string? savedUrl = null;

                await using var trx = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (item.formFile == null || item.formFile.Length == 0)
                    {
                        res.Status = SubmissionStatus.Failed;
                        res.Messages.Add("Form file is required.");
                        results.Add(res);
                        continue;
                    }

                    // Get expected return and validate
                    var expected = await _context.ExpectedReturns
                        .Include(er => er.ReturnForm)
                        .FirstOrDefaultAsync(er => er.Id == item.ExpectedReturnId);

                    if (expected == null || expected.ReturnForm.SaccoTypeId != loggedInSacco.SaccoType)
                    {
                        res.Status = SubmissionStatus.Failed;
                        res.Messages.Add("Invalid form or not allowed for your Sacco type.");
                        results.Add(res);
                        continue;
                    }

                    // Find the current latest submission (if any) for this return
                    var previous = await _context.ReturnSubmissions
                        .Where(s => s.ExpectedReturnId == item.ExpectedReturnId
                                    && s.SaccoId == loggedInSacco.SaccoId
                                    && s.IsLatest)
                        .SingleOrDefaultAsync();

                    // If IsAmendment is true but no previous submission exists, treat as error or fallback to Draft
                    if (IsAmendment && previous == null)
                    {
                        res.Status = SubmissionStatus.Failed;
                        res.Messages.Add("Cannot process amendment without a previous submission.");
                        results.Add(res);
                        continue;
                    }

                    // Save file (non-DB: we'll delete on rollback if needed)
                    savedUrl = await FormsHelper.SaveFileAsync(item.formFile, "Returns");
                    if (savedUrl == null)
                    {
                        res.Status = SubmissionStatus.Failed;
                        res.Messages.Add("Could not store file.");
                        results.Add(res);
                        continue;
                    }

                    // Determine the status for the new submission
                    string newStatus = IsAmendment ? previous!.Status : SubmissionStatus.Draft.ToString();

                    // Create the new submission (version = prev.Version + 1)
                    var submission = new ReturnSubmission
                    {
                        ExpectedReturnId = item.ExpectedReturnId,
                        SaccoId = loggedInSacco.SaccoId,
                        Status = newStatus,
                        SubmittedAt = DateTime.Now,
                        FileUrl = savedUrl,
                        Version = (previous?.Version ?? 0) + 1,
                        AmendsSubmissionId = previous?.Id
                    };

                    if (previous != null)
                    {
                        previous.IsLatest = false;
                        previous.AmendedBySubmissionId = submission.Id;
                        _context.ReturnSubmissions.Entry(previous).State = EntityState.Modified;
                        _context.ReturnSubmissions.Update(previous);
                    }

                    await _context.ReturnSubmissions.AddAsync(submission);
                    await _context.SaveChangesAsync();  // Save submission inside transaction

                    res.SubmissionId = submission.Id;

                    FormCategory Category = (FormCategory)expected.ReturnForm.Category;

                    // Parse the Excel file
                    var parse = await _excelParser.ParseAsync(item.formFile, Category, loggedInSacco.SaccoType);

                    if (!parse.Success)
                    {
                        res.Status = SubmissionStatus.Failed;
                        res.Messages.AddRange(parse.Errors);
                        throw new Exception("Parsing failed");  // Trigger rollback
                    }

                    // Process parsed rows and save entities
                    foreach (var row in parse.Rows)
                    {
                        row.ReturnSubmissionId = submission.Id;
                        var entity = row.ToEntity();
                        await AddEntityToSubmission(submission, entity);
                    }

                    await _context.SaveChangesAsync();  // Save entities inside transaction

                    await trx.CommitAsync();  // Commit if all succeeds

                    res.Status = Enum.Parse<SubmissionStatus>(newStatus);  // Assuming SubmissionStatus enum matches the string values
                    res.Messages.Add(previous == null
                                     ? $"Saved as {newStatus.ToLower()}."
                                     : $"Saved as {newStatus.ToLower()} – supersedes v{previous.Version}.");
                    results.Add(res);
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync();  // Rollback DB changes

                    // Manual file rollback
                    if (!string.IsNullOrEmpty(savedUrl))
                    {
                        //FormsHelper.DeleteFile(savedUrl); 
                    }

                    _logger.LogError(ex, $"Error processing form {item.FormId}");
                    res.Status = SubmissionStatus.Failed;
                    res.Messages.Add($"Error processing form: {ex.Message}");
                    results.Add(res);
                }
            }

            return results;
        }

        private async Task AddEntityToSubmission(ReturnSubmission submission, object entity)
        {
            switch (entity)
            {
                // DT Returns
                case DTCapitalAdequacyReturn capitalAdequacy:
                    submission.DTCapitalAdequacyReturns.Add(capitalAdequacy);
                    break;

                case DTLiquidityReturn liquidity:
                    submission.DTLiquidityReturns.Add(liquidity);
                    break;

                case DepositReturn deposit:
                    submission.DepositReturns.Add(deposit);
                    break;

                case DTRiskClassificationReturn risk:
                    submission.DTRiskClassificationReturns.Add(risk);
                    break;

                case DTInvestmentReturn investment:
                    submission.DTInvestmentReturns.Add(investment);
                    break;

                case DTFinancialPositionReturn financialPosition:
                    submission.DTFinancialPositionReturns.Add(financialPosition);
                    break;

                case DTComprehensiveIncomeReturn comprehensiveIncome:
                    submission.DTComprehensiveIncomeReturns.Add(comprehensiveIncome);
                    break;

                // NWDT Returns
                case NWDTCapitalAdequacyReturn nwdtCapitalAdequacy:
                    submission.NWDTCapitalAdequacyReturns.Add(nwdtCapitalAdequacy);
                    break;

                case NWDTLiquidityReturn nwdtLiquidity:
                    submission.NWDTLiquidityReturns.Add(nwdtLiquidity);
                    break;

                case NWDTDepositReturn nwdtDeposit:
                    submission.NWDTDepositReturns.Add(nwdtDeposit);
                    break;

                case NWDTRiskClassificationReturn nwdtRisk:
                    submission.NWDTRiskClassificationReturns.Add(nwdtRisk);
                    break;

                case NWDTInvestmentReturn nwdtInvestment:
                    submission.NWDTInvestmentReturns.Add(nwdtInvestment);
                    break;

                case NWDTFinancialPositionReturn nwdtFinancialPosition:
                    submission.NWDTFinancialPositionReturns.Add(nwdtFinancialPosition);
                    break;

                case NWDTComprehensiveIncomeReturn nwdtComprehensiveIncome:
                    submission.NWDTComprehensiveIncomeReturns.Add(nwdtComprehensiveIncome);
                    break;

                // For collections (like multiple deposit returns), handle differently
                case List<DepositReturn> depositList:
                    foreach (var depositItem in depositList)
                    {
                        submission.DepositReturns.Add(depositItem);
                    }
                    break;

                case List<NWDTDepositReturn> nwdtDepositList:
                    foreach (var nwdtDepositItem in nwdtDepositList)
                    {
                        submission.NWDTDepositReturns.Add(nwdtDepositItem);
                    }
                    break;

                case List<DTRiskClassificationReturn> riskList:
                    foreach (var riskItem in riskList)
                    {
                        submission.DTRiskClassificationReturns.Add(riskItem);
                    }
                    break;

                case List<NWDTRiskClassificationReturn> nwdtRiskList:
                    foreach (var nwdtRiskItem in nwdtRiskList)
                    {
                        submission.NWDTRiskClassificationReturns.Add(nwdtRiskItem);
                    }
                    break;

                // Management Return - stored as separate entity
                case ManagementReturn managementReturn:
                    await _context.ManagementReturns.AddAsync(managementReturn);
                    break;

                // Daily Liquidity Return - stored as separate entity
                case DailyLiquidityReturn dailyLiquidityReturn:
                    await _context.DailyLiquidityReturns.AddAsync(dailyLiquidityReturn);
                    break;

                // Sectoral Lending Report - returns anonymous object with Report and EconomicSectorData
                case var sectoralLending when IsSectoralLendingObject(entity):
                    try
                    {
                        var report = entity.GetType().GetProperty("Report")?.GetValue(entity);
                        var economicSectorData = entity.GetType().GetProperty("EconomicSectorData")?.GetValue(entity);

                        if (report is SectoralLendingReport sectoralReport)
                        {
                            // Save the report first to get its ID
                            await _context.SectoralLendingReports.AddAsync(sectoralReport);
                            await _context.SaveChangesAsync(); // Save to get the ID

                            // Now update the EconomicSectorData records with the report ID
                            if (economicSectorData is List<EconomicSectorData> sectorDataList)
                            {
                                foreach (var sectorData in sectorDataList)
                                {
                                    sectorData.SectoralLendingReportId = sectoralReport.Id;
                                }
                                await _context.SectoralLendingData.AddRangeAsync(sectorDataList);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing sectoral lending data");
                    }
                    break;

                // Insider Lending - returns anonymous object with Header and InsiderLoans
                case var insiderLending when IsInsiderLendingObject(entity):
                    try
                    {
                        var header = entity.GetType().GetProperty("Header")?.GetValue(entity);
                        var insiderLoans = entity.GetType().GetProperty("InsiderLoans")?.GetValue(entity);

                        if (header is InsiderLendingHeader insiderHeader)
                        {
                            await _context.InsiderLendingHeaders.AddAsync(insiderHeader);
                        }

                        if (insiderLoans is List<InsiderLoan> loansList)
                        {
                            await _context.InsiderLoans.AddRangeAsync(loansList);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing insider lending data");
                    }
                    break;

                default:
                    _logger.LogWarning($"Unknown entity type: {entity?.GetType()?.Name ?? "null"}");
                    break;
            }
        }

        public async Task<IList<SubmissionResultDto>> SubmitFinalAsync(string submissionId)
        {
            var results = new List<SubmissionResultDto>();

            try
            {
                var submission = await _context.ReturnSubmissions
                    .FirstOrDefaultAsync(s => s.Id == submissionId);

                if (submission == null)
                {
                    results.Add(new SubmissionResultDto
                    {
                        SubmissionId = submissionId,
                        Status = SubmissionStatus.Failed,
                        Messages = new List<string> { "Submission not found." }
                    });
                    return results;
                }

                // Perform any final validation here
                // ...

                // Update status or perform any final processing
                submission.IsActive = true;
                await _context.SaveChangesAsync();

                results.Add(new SubmissionResultDto
                {
                    SubmissionId = submissionId,
                    Status = SubmissionStatus.Submitted,
                    Messages = new List<string> { "Successfully submitted." }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting submission {submissionId}");
                results.Add(new SubmissionResultDto
                {
                    SubmissionId = submissionId,
                    Status = SubmissionStatus.Failed,
                    Messages = new List<string> { $"Error submitting: {ex.Message}" }
                });
            }

            return results;
        }

        /// <summary>
        /// Get submission statuses for multiple expected returns efficiently with caching
        /// </summary>
        /// <param name="expectedReturnIds">List of expected return IDs</param>
        /// <returns>Dictionary mapping expected return ID to submission data</returns>
        public async Task<Dictionary<string, (SubmissionStatus Status, string? SubmissionId, DateTime? SubmittedAt, string? FileUrl)>> GetSubmissionStatusesAsync(List<string> expectedReturnIds, Boolean useCache = true)
        {
            if (!expectedReturnIds.Any())
                return new Dictionary<string, (SubmissionStatus, string?, DateTime?, string?)>();

            var result = new Dictionary<string, (SubmissionStatus, string?, DateTime?, string?)>();
            var uncachedIds = new List<string>();

            // Check cache first if useCache is true
            if (useCache)
            {
                foreach (var expectedReturnId in expectedReturnIds)
                {
                    var cacheKey = $"{SubmissionStatusCacheKey}{expectedReturnId}";
                    if (_cache.TryGetValue(cacheKey, out var cachedData))
                    {
                        result[expectedReturnId] = ((SubmissionStatus, string?, DateTime?, string?))cachedData;
                    }
                    else
                    {
                        uncachedIds.Add(expectedReturnId);
                    }
                }
            }
            else
            {
                uncachedIds.AddRange(expectedReturnIds); // Bypass cache if useCache is false
            }

            // If all data was cached and useCache is true, return immediately
            if (!uncachedIds.Any() && useCache)
                return result;

            // Fetch uncached data from database
            var submissions = await _context.ReturnSubmissions
                .Where(rs => uncachedIds.Contains(rs.ExpectedReturnId) && rs.IsActive && rs.IsLatest)
                .Select(rs => new
                {
                    rs.ExpectedReturnId,
                    rs.Id,
                    rs.FileUrl,
                    rs.Status,
                    rs.SubmittedAt,
                    HasData = rs.DTCapitalAdequacyReturns.Any() ||
                              rs.DTComprehensiveIncomeReturns.Any() ||
                              rs.DTFinancialPositionReturns.Any() ||
                              rs.DTInvestmentReturns.Any() ||
                              rs.DTLiquidityReturns.Any() ||
                              rs.DTRiskClassificationReturns.Any() ||
                              rs.DepositReturns.Any() ||
                              rs.NWDTCapitalAdequacyReturns.Any() ||
                              rs.NWDTLiquidityReturns.Any() ||
                              rs.NWDTDepositReturns.Any() ||
                              rs.NWDTInvestmentReturns.Any() ||
                              rs.NWDTFinancialPositionReturns.Any() ||
                              rs.NWDTComprehensiveIncomeReturns.Any() ||
                              rs.NWDTRiskClassificationReturns.Any()
                })
                .ToListAsync();

            // Process uncached data
            foreach (var expectedReturnId in uncachedIds)
            {
                var latestSubmission = submissions
                    .Where(s => s.ExpectedReturnId == expectedReturnId)
                    .OrderByDescending(s => s.SubmittedAt)
                    .FirstOrDefault();

                (SubmissionStatus Status, string? SubmissionId, DateTime? SubmittedAt, string? FileUrl) submissionData;

                if (latestSubmission == null)
                {
                    submissionData = (SubmissionStatus.NotSubmitted, null, null,null);
                }
                else
                {
                    SubmissionStatus status;
                    // Parse the status from the string field
                    if (Enum.TryParse<ExpectedStatus>(latestSubmission.Status, out var parsedStatus))
                    {
                        status = parsedStatus switch
                        {
                            ExpectedStatus.Filed => SubmissionStatus.Submitted,
                            ExpectedStatus.Draft => SubmissionStatus.Draft,
                            _ => SubmissionStatus.NotSubmitted
                        };
                    }
                    else
                    {
                        // Fallback: check if submission has data to determine if it's a draft
                        status = latestSubmission.HasData ? SubmissionStatus.Draft : SubmissionStatus.NotSubmitted;
                    }

                    submissionData = (status, latestSubmission.Id, latestSubmission.SubmittedAt, latestSubmission.FileUrl);
                }

                // Cache the result only if useCache is true
                if (useCache)
                {
                    var cacheKey = $"{SubmissionStatusCacheKey}{expectedReturnId}";
                    _cache.Set(cacheKey, submissionData, TimeSpan.FromSeconds(10));
                }

                result[expectedReturnId] = submissionData;
            }

            return result;
        }
        /// <summary>
        /// Check if a form is late based on filing deadline
        /// </summary>
        /// <param name="filingDeadline">The filing deadline</param>
        /// <param name="status">Current submission status</param>
        /// <returns>True if the form is late</returns>
        public bool IsFormLate(DateTime filingDeadline, SubmissionStatus status)
        {
            var currentDate = DateTime.Now;
            return currentDate > filingDeadline && status != SubmissionStatus.Submitted;
        }

        /// <summary>
        /// Bulk submit all returns for a specific period
        /// </summary>
        /// <param name="periodId">The period ID</param>
        /// <param name="saccoId">The SACCO ID</param>
        /// <param name="saccoType">The SACCO type</param>
        /// <returns>Bulk submission result</returns>
        // DTO stays the same

        // ------------ ReturnSubmissionService ------------
        public async Task<BulkSubmissionResultDTO> BulkSubmitByPeriodAsync(
            string periodId, string saccoId, string saccoType)
        {
            var result = new BulkSubmissionResultDTO();

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var expectedReturns = await _context.ExpectedReturns
                    .Include(er => er.ReturnForm)
                    .Include(er => er.ReturnSubmissions.Where(rs => rs.IsActive))
                    .Where(er => er.PeriodId == periodId &&
                                 er.IsActive &&
                                 er.ReturnForm.SaccoTypeId == saccoType)
                    .ToListAsync();

                if (!expectedReturns.Any())
                {
                    result.Success = false;
                    result.Message = "No expected returns found for the specified period.";
                    return result;
                }

                var ids = expectedReturns.Select(er => er.Id).ToList();
                var sts = await GetSubmissionStatusesAsync(ids, useCache: false);

                var drafts = expectedReturns.Where(er =>
                               sts.GetValueOrDefault(er.Id).Status == SubmissionStatus.Draft)
                               .ToList();

                if (!drafts.Any())
                {
                    result.Success = false;
                    result.Message = "No draft returns found for bulk submission.";
                    return result;
                }

                result.TotalForms = drafts.Count;

                foreach (var er in drafts)
                {
                    var latest = er.ReturnSubmissions
                                   .OrderByDescending(rs => rs.SubmittedAt)
                                   .FirstOrDefault();

                    if (latest == null)
                        throw new Exception("No draft submission found.");

                    latest.Status = SubmissionStatus.Submitted.ToString();
                    latest.SubmittedAt = DateTime.UtcNow;
                    _context.Entry(latest).State = EntityState.Modified;

                    result.Details
                        .Add(new BulkSubmissionDetailDTO
                            {
                                SubmissionId = latest.Id,
                                Success = true
                            });
                }

                await _context.SaveChangesAsync();

                // completeness check
                var (isComplete, missCnt) = await UpdateReturnCompletenessAsync(
                                                periodId, saccoId, saccoType);

                result.Success = true;
                result.Message = $"Successfully submitted {drafts.Count} forms.";
                result.SuccessfullySubmitted = drafts.Count;
                result.IsPeriodComplete = isComplete;
                result.MissingCount = missCnt;

                await tx.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error in bulk submission for period {PeriodId}", periodId);
                result.Success = false;
                result.Message = $"An error occurred during bulk submission: {ex.Message}";
                result.FailedSubmissions = result.TotalForms - result.SuccessfullySubmitted;
                return result;
            }
        }

        // (isComplete, missingCount)
        private async Task<(bool, int)> UpdateReturnCompletenessAsync(
            string periodId, string saccoId, string saccoType)
        {
            var expectedCodes = await _context.ExpectedReturns
                .Where(er => er.PeriodId == periodId && er.ReturnForm.SaccoTypeId == saccoType)
                .Select(er => er.ReturnForm.Code)
                .ToListAsync();

            var submitted = await _context.ReturnSubmissions
                .Include(rs => rs.ExpectedReturn.ReturnForm)
                .Where(rs => rs.SaccoId == saccoId &&
                             rs.ExpectedReturn.PeriodId == periodId &&
                             rs.Status == SubmissionStatus.Submitted.ToString() &&
                             rs.IsLatest && rs.IsActive)
                .ToListAsync();

            var submittedCodes = submitted.Select(s => s.ExpectedReturn.ReturnForm.Code).ToList();
            var missing = expectedCodes.Except(submittedCodes).ToList();
            var complete = !missing.Any();

            var rc = await _context.ReturnCompleteness
                .FirstOrDefaultAsync(x => x.PeriodId == periodId && x.SaccoId == saccoId);

            if (rc == null)
            {
                rc = new ReturnCompleteness { PeriodId = periodId, SaccoId = saccoId };
                await _context.ReturnCompleteness.AddAsync(rc);
            }

            rc.IsComplete = complete;
            rc.ExpectedReturnCount = expectedCodes.Count;
            rc.ActualReturnCount = submittedCodes.Count;
            rc.MissingForms = JsonSerializer.Serialize(missing);
            rc.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (!complete)
            {
                BackgroundJob.Enqueue(
                    () => SendIncompleteSubmissionNotificationAsync(
                                saccoId, periodId, missing.Count));
            }
                

            return (complete, missing.Count);
        }


        private bool IsSectoralLendingObject(object entity)
        {
            if (entity == null) return false;

            var type = entity.GetType();
            return type.Name.Contains("AnonymousType") &&
                   type.GetProperty("Report") != null &&
                   type.GetProperty("EconomicSectorData") != null;
        }

        private bool IsInsiderLendingObject(object entity)
        {
            if (entity == null) return false;

            var type = entity.GetType();
            return type.Name.Contains("AnonymousType") &&
                   type.GetProperty("Header") != null &&
                   type.GetProperty("InsiderLoans") != null;
        }

        public async Task SendSubmissionConfirmationEmailAsync(string saccoId, string periodId)
        {
            try
            {
                var sacco = await complianceService.GetSaccoByIdAsync(saccoId);
                if (sacco == null || string.IsNullOrEmpty(sacco.OfficialSaccoEmail))
                {
                    _logger.LogWarning("Sacco {SaccoId} not found or no email for submission confirmation.", saccoId);
                    return;
                }

                var period = await _context.ReturnPeriods.FindAsync(periodId);
                if (period == null)
                {
                    _logger.LogWarning("Period {PeriodId} not found for submission confirmation.", periodId);
                    return;
                }

                var subject = "Returns Submission Received";
                var message = $"Dear {sacco.SaccoName},\n\nWe have received your returns for the period {period.Name}.\n\nThank you for your submission.\n\nBest regards,\nSASRA Team";

                await _emailService.SendEmailAsync(sacco.OfficialSaccoEmail, subject, message);

                _logger.LogInformation("Submission confirmation email sent to Sacco {SaccoId} for period {PeriodId}.", saccoId, periodId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending submission confirmation email for Sacco {SaccoId} and period {PeriodId}.", saccoId, periodId);
            }
        }
        public async Task SendIncompleteSubmissionNotificationAsync(string saccoId, string periodId, int missingCount)
        {
            try
            {
                var sacco = await complianceService.GetSaccoByIdAsync(saccoId);
                if (sacco == null || string.IsNullOrEmpty(sacco.OfficialSaccoEmail))
                {
                    _logger.LogWarning("Sacco {SaccoId} not found or no email.", saccoId);
                    return;
                }

                var period = await _context.ReturnPeriods.FindAsync(periodId);
                var subject = "Incomplete Quarterly Return Submission";
                var message = $"Your quarterly return submission for {period?.Name ?? periodId} is incomplete. {missingCount} forms are missing. Please submit the remaining forms.";

                await _emailService.SendEmailAsync(sacco.OfficialSaccoEmail, subject, message);
                _logger.LogInformation("Incomplete submission notification sent to Sacco {SaccoId} for period {PeriodId}.", saccoId, periodId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending incomplete submission notification for Sacco {SaccoId} and period {PeriodId}.", saccoId, periodId);
            }
        }
    }
}
