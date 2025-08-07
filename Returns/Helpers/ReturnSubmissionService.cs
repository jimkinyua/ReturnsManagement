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

        public async Task<IList<SubmissionResultDto>> UploadDraftAsync(NewReturnDTO dto, LoggedInEntity loggedInSacco, Boolean IsAmendment = false)
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
                        IsLatest = true,
                        IsActive = true,
                        FileUrl = savedUrl,
                        Version = (previous?.Version ?? 0) + 1,
                        AmendsSubmissionId = previous?.Id
                    };

                    if (previous != null)
                    {
                        previous.IsLatest = false;
                        IsAmendment = true;
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

                    // Update children status (mark as current/inactive based on amendment status)
                    await UpdateChildrenStatusAsync(submission, IsAmendment);

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
                    capitalAdequacy.IsCurrent = true;
                    capitalAdequacy.IsAmended = false;
                    submission.DTCapitalAdequacyReturns.Add(capitalAdequacy);
                    break;

                case AuditedComprehensiveIncome auditedComprehensiveIncome:
                    auditedComprehensiveIncome.IsCurrent = true;
                    auditedComprehensiveIncome.IsAmended = false;
                    submission.AuditedComprehensiveIncomes.Add(auditedComprehensiveIncome);
                    break;

                case DTLiquidityReturn liquidity:
                    liquidity.IsCurrent = true;
                    liquidity.IsAmended = false;
                    submission.DTLiquidityReturns.Add(liquidity);
                    break;

                case DepositReturn deposit:
                    deposit.IsCurrent = true;
                    deposit.IsAmended = false;
                    submission.DepositReturns.Add(deposit);
                    break;

                case DTRiskClassificationReturn risk:
                    risk.IsCurrent = true;
                    risk.IsAmended = false;
                    submission.DTRiskClassificationReturns.Add(risk);
                    break;

                case DTInvestmentReturn investment:
                    investment.IsCurrent = true;
                    investment.IsAmended = false;
                    submission.DTInvestmentReturns.Add(investment);
                    break;

                case DTFinancialPositionReturn financialPosition:
                    financialPosition.IsCurrent = true;
                    financialPosition.IsAmended = false;
                    submission.DTFinancialPositionReturns.Add(financialPosition);
                    break;

                case DTComprehensiveIncomeReturn comprehensiveIncome:
                    comprehensiveIncome.IsCurrent = true;
                    comprehensiveIncome.IsAmended = false;
                    submission.DTComprehensiveIncomeReturns.Add(comprehensiveIncome);
                    break;

                // NWDT Returns
                case NWDTCapitalAdequacyReturn nwdtCapitalAdequacy:
                    nwdtCapitalAdequacy.IsCurrent = true;
                    nwdtCapitalAdequacy.IsAmended = false;
                    submission.NWDTCapitalAdequacyReturns.Add(nwdtCapitalAdequacy);
                    break;

                case NWDTLiquidityReturn nwdtLiquidity:
                    nwdtLiquidity.IsCurrent = true;
                    nwdtLiquidity.IsAmended = false;
                    submission.NWDTLiquidityReturns.Add(nwdtLiquidity);
                    break;

                case NWDTDepositReturn nwdtDeposit:
                    nwdtDeposit.IsCurrent = true;
                    nwdtDeposit.IsAmended = false;
                    submission.NWDTDepositReturns.Add(nwdtDeposit);
                    break;

                case NWDTRiskClassificationReturn nwdtRisk:
                    nwdtRisk.IsCurrent = true;
                    nwdtRisk.IsAmended = false;
                    submission.NWDTRiskClassificationReturns.Add(nwdtRisk);
                    break;

                case NWDTInvestmentReturn nwdtInvestment:
                    nwdtInvestment.IsCurrent = true;
                    nwdtInvestment.IsAmended = false;
                    submission.NWDTInvestmentReturns.Add(nwdtInvestment);
                    break;

                case NWDTFinancialPositionReturn nwdtFinancialPosition:
                    nwdtFinancialPosition.IsCurrent = true;
                    nwdtFinancialPosition.IsAmended = false;
                    submission.NWDTFinancialPositionReturns.Add(nwdtFinancialPosition);
                    break;

                case NWDTComprehensiveIncomeReturn nwdtComprehensiveIncome:
                    nwdtComprehensiveIncome.IsCurrent = true;
                    nwdtComprehensiveIncome.IsAmended = false;
                    submission.NWDTComprehensiveIncomeReturns.Add(nwdtComprehensiveIncome);
                    break;

                // For collections (like multiple deposit returns), handle differently
                case List<DepositReturn> depositList:
                    foreach (var depositItem in depositList)
                    {
                        depositItem.IsCurrent = true;
                        depositItem.IsAmended = false;
                        submission.DepositReturns.Add(depositItem);
                    }
                    break;

                case List<NWDTDepositReturn> nwdtDepositList:
                    foreach (var nwdtDepositItem in nwdtDepositList)
                    {
                        nwdtDepositItem.IsCurrent = true;
                        nwdtDepositItem.IsAmended = false;
                        submission.NWDTDepositReturns.Add(nwdtDepositItem);
                    }
                    break;

                case List<DTRiskClassificationReturn> riskList:
                    foreach (var riskItem in riskList)
                    {
                        riskItem.IsCurrent = true;
                        riskItem.IsAmended = false;
                        submission.DTRiskClassificationReturns.Add(riskItem);
                    }
                    break;

                case List<AuditedRiskClassification> riskList:
                    foreach (var riskItem in riskList)
                    {
                        riskItem.IsCurrent = true;
                        riskItem.IsAmended = false;
                        submission.AuditedRiskClassifications.Add(riskItem);
                    }
                    break;

                case List<NWDTRiskClassificationReturn> nwdtRiskList:
                    foreach (var nwdtRiskItem in nwdtRiskList)
                    {
                        nwdtRiskItem.IsCurrent = true;
                        nwdtRiskItem.IsAmended = false;
                        submission.NWDTRiskClassificationReturns.Add(nwdtRiskItem);
                    }
                    break;

                // Management Return - stored as separate entity
                case ManagementReturn managementReturn:
                    managementReturn.IsCurrent = true;
                    managementReturn.IsAmended = false;
                    await _context.ManagementReturns.AddAsync(managementReturn);
                    break;

                // Daily Liquidity Return - stored as separate entity
                case DailyLiquidityReturn dailyLiquidityReturn:
                    dailyLiquidityReturn.IsCurrent = true;
                    dailyLiquidityReturn.IsAmended = false;
                    await _context.DailyLiquidityReturns.AddAsync(dailyLiquidityReturn);
                    break;

                case AuditedFinancialPosition auditedFinancialPosition:
                    auditedFinancialPosition.IsCurrent = true;
                    auditedFinancialPosition.IsAmended = false;
                    await _context.AuditedFinancialPositions.AddAsync(auditedFinancialPosition);
                    break;
                // Sectoral Lending Report - returns anonymous object with Report and EconomicSectorData
                case var sectoralLending when IsSectoralLendingObject(entity):
                    try
                    {
                        var report = entity.GetType().GetProperty("Report")?.GetValue(entity);
                        var economicSectorData = entity.GetType().GetProperty("EconomicSectorData")?.GetValue(entity);

                        if (report is SectoralLendingReport sectoralReport)
                        {
                            sectoralReport.IsCurrent = true;
                            sectoralReport.IsAmended = false;
                            sectoralReport.SaccoId = sectoralReport.SaccoId; // Use existing SaccoId from report
                            // Save the report first to get its ID
                            await _context.SectoralLendingReports.AddAsync(sectoralReport);
                            await _context.SaveChangesAsync(); // Save to get the ID

                            // Now update the EconomicSectorData records with the report ID and SaccoId
                            if (economicSectorData is List<EconomicSectorData> sectorDataList)
                            {
                                foreach (var sectorData in sectorDataList)
                                {
                                    sectorData.SectoralLendingReportId = sectoralReport.Id;
                                    sectorData.SaccoCsNumber = sectoralReport.SaccoId;
                                    sectorData.StartDate = sectoralReport.StartDate;
                                    sectorData.EndDate = sectoralReport.EndDate;
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
                            insiderHeader.IsCurrent = true;
                            insiderHeader.IsAmended = false;
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

        /// <summary>
        /// Updates the IsActive status for all children entities in a submission
        /// This ensures that when a new submission is created, all previous children are marked as inactive
        /// and the new children are marked as active
        /// </summary>
        private async Task UpdateChildrenStatusAsync(ReturnSubmission submission, bool isAmendment)
        {
            try
            {
                if (!isAmendment)
                {
                    // For new submissions, mark all new children as active
                    MarkChildrenAsActive(submission);
                }
                else
                {
                    // For amendments, mark previous children as inactive and new children as active
                    await MarkPreviousChildrenAsInactiveAsync(submission);
                    MarkChildrenAsActive(submission);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating children status for submission {SubmissionId}", submission.Id);
                throw;
            }
        }

        /// <summary>
        /// Marks all children entities in the submission as active (IsCurrent = true)
        /// </summary>
        private void MarkChildrenAsActive(ReturnSubmission submission)
        {
            // For new entities, we don't need to explicitly mark them as Modified
            // since they are already being tracked as Added by EF Core
            // The IsCurrent and IsAmended properties are set during entity creation in AddEntityToSubmission

            // This method is kept for consistency but the actual marking is done in AddEntityToSubmission
            // when the entities are first created
        }

        /// <summary>
        /// Marks all previous children entities as inactive (IsCurrent = false, IsAmended = true)
        /// This is called when processing amendments to ensure previous versions are properly marked
        /// </summary>
        private async Task MarkPreviousChildrenAsInactiveAsync(ReturnSubmission submission)
        {
            if (submission.AmendsSubmissionId == null)
                return;

            try
            {
                // Use direct SQL updates to avoid concurrency issues
                var previousSubmissionId = submission.AmendsSubmissionId;

                // Update DT Capital Adequacy Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE DTCapitalAdequacyReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update Audited Risk Classifications
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE AuditedRiskClassifications SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // update SectoralLendingData
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE SectoralLendingData SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update Audited Comprehensive Incomes
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE AuditedComprehensiveIncomes SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update Audited Financial Positions
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE AuditedFinancialPositions SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update DT Liquidity Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE DTLiquidityReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update Deposit Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE DepositReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update DT Risk Classification Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE DTRiskClassificationReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                await _context.Database.ExecuteSqlRawAsync(
                  "UPDATE AuditedRiskClassifications SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                  previousSubmissionId);

                // Update DT Investment Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE DTInvestmentReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update DT Financial Position Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE DTFinancialPositionReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update DT Comprehensive Income Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE DTComprehensiveIncomeReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update NWDT Capital Adequacy Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE NWDTCapitalAdequacyReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update NWDT Liquidity Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE NWDTLiquidityReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update NWDT Deposit Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE NWDTDepositReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update NWDT Risk Classification Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE NWDTRiskClassificationReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update NWDT Investment Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE NWDTInvestmentReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update NWDT Financial Position Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE NWDTFinancialPositionReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                // Update NWDT Comprehensive Income Returns
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE NWDTComprehensiveIncomeReturns SET IsCurrent = 0, IsAmended = 1 WHERE ReturnSubmissionId = {0}",
                    previousSubmissionId);

                _logger.LogInformation("Successfully marked previous children as inactive for submission {SubmissionId}", submission.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking previous children as inactive for submission {SubmissionId}", submission.Id);
                // Don't throw the exception - this is not critical for the main operation
                // The new entities will still be marked as active
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
                    submissionData = (SubmissionStatus.NotSubmitted, null, null, null);
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

                // Check if all returns are already submitted
                var submittedReturns = expectedReturns.Where(er =>
                    sts.GetValueOrDefault(er.Id).Status == SubmissionStatus.Submitted)
                    .ToList();

                if (!drafts.Any() && submittedReturns.Count == expectedReturns.Count)
                {
                    result.Success = false;
                    result.Message = "All returns for this period have already been submitted.";
                    result.TotalForms = expectedReturns.Count;
                    result.SuccessfullySubmitted = 0;
                    result.FailedSubmissions = expectedReturns.Count;
                    return result;
                }

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
                long LongsaccoId = long.Parse(saccoId);
                var sacco = await complianceService.GetSaccoByIdAsync(LongsaccoId);
                if (sacco == null || string.IsNullOrEmpty(sacco.OfficialEmail))
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

                await _emailService.SendEmailAsync(sacco.OfficialEmail, subject, message);

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
                long LongsaccoId = long.Parse(saccoId);
                var sacco = await complianceService.GetSaccoByIdAsync(LongsaccoId);
                if (sacco == null || string.IsNullOrEmpty(sacco.OfficialEmail))
                {
                    _logger.LogWarning("Sacco {SaccoId} not found or no email.", saccoId);
                    return;
                }

                var period = await _context.ReturnPeriods.FindAsync(periodId);
                var subject = "Incomplete Quarterly Return Submission";
                var message = $"Your quarterly return submission for {period?.Name ?? periodId} is incomplete. {missingCount} forms are missing. Please submit the remaining forms.";

                await _emailService.SendEmailAsync(sacco.OfficialEmail, subject, message);
                _logger.LogInformation("Incomplete submission notification sent to Sacco {SaccoId} for period {PeriodId}.", saccoId, periodId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending incomplete submission notification for Sacco {SaccoId} and period {PeriodId}.", saccoId, periodId);
            }
        }
    }
}
