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

namespace Returns.Helpers
{
    public class ReturnSubmissionService : IReturnSubmissionService
    {
        private readonly ReturnsDbContext _context;
        private readonly IExcelParser _excelParser;
        private readonly ILogger<ReturnSubmissionService> _logger;
        private readonly IMemoryCache _cache;
        private const string SubmissionStatusCacheKey = "SubmissionStatus_";

        public ReturnSubmissionService(
            ReturnsDbContext context,
            IExcelParser excelParser,
            ILogger<ReturnSubmissionService> logger,
            IMemoryCache cache)
        {
            _context = context;
            _excelParser = excelParser;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IList<SubmissionResultDto>> UploadDraftAsync(NewReturnDTO dto, LoggedInEntity loggedInSacco)
        {
            var results = new List<SubmissionResultDto>();

            foreach (var item in dto.FormUploads)
            {
                var res = new SubmissionResultDto();
                res.FormFileName = item.formFile?.FileName ?? "Unknown";

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

                    // Save file
                    var url = await FormsHelper.SaveFileAsync(item.formFile, "Returns");
                    if (url == null)
                    {
                        res.Status = SubmissionStatus.Failed;
                        res.Messages.Add("Could not store file.");
                        results.Add(res);
                        continue;
                    }

                    // Create submission record
                    var submission = new ReturnSubmission
                    {
                        ExpectedReturnId = item.ExpectedReturnId,
                        SaccoId = loggedInSacco.SaccoId,
                        Status = SubmissionStatus.Draft.ToString(),
                        SubmittedAt = DateTime.Now,
                        FileUrl = url,
                    };

                    await _context.ReturnSubmissions.AddAsync(submission);
                    await _context.SaveChangesAsync();
                    res.SubmissionId = submission.Id;

                    FormCategory Category = (FormCategory)expected.ReturnForm.Category;

                    // Parse the Excel file
                    var parse = await _excelParser.ParseAsync(item.formFile, Category, loggedInSacco.SaccoType);

                    if (!parse.Success)
                    {
                        res.Status = SubmissionStatus.Failed;
                        res.Messages.AddRange(parse.Errors);

                        // Remove the submission if parsing failed
                        _context.ReturnSubmissions.Remove(submission);
                        await _context.SaveChangesAsync();

                        results.Add(res);
                        continue;
                    }

                    // Process parsed rows and save entities
                    foreach (var row in parse.Rows)
                    {
                        row.ReturnSubmissionId = submission.Id;
                        var entity = row.ToEntity();

                        // Add entity to appropriate collection based on type
                        await AddEntityToSubmission(submission, entity);
                    }

                    await _context.SaveChangesAsync();

                    res.Status = SubmissionStatus.Draft;
                    res.Messages.Add("Saved as draft.");
                    results.Add(res);
                }
                catch (Exception ex)
                {
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
        public async Task<Dictionary<string, (SubmissionStatus Status, string? SubmissionId, DateTime? SubmittedAt)>> GetSubmissionStatusesAsync(List<string> expectedReturnIds)
        {
            if (!expectedReturnIds.Any())
                return new Dictionary<string, (SubmissionStatus, string?, DateTime?)>();

            var result = new Dictionary<string, (SubmissionStatus, string?, DateTime?)>();
            var uncachedIds = new List<string>();

            // Check cache first
            foreach (var expectedReturnId in expectedReturnIds)
            {
                var cacheKey = $"{SubmissionStatusCacheKey}{expectedReturnId}";
                if (_cache.TryGetValue(cacheKey, out var cachedData))
                {
                    result[expectedReturnId] = ((SubmissionStatus, string?, DateTime?))cachedData;
                }
                else
                {
                    uncachedIds.Add(expectedReturnId);
                }
            }

            // If all data was cached, return immediately
            if (!uncachedIds.Any())
                return result;

            // Fetch uncached data from database
            var submissions = await _context.ReturnSubmissions
                .Where(rs => uncachedIds.Contains(rs.ExpectedReturnId) && rs.IsActive)
                .Select(rs => new
                {
                    rs.ExpectedReturnId,
                    rs.Id,
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

                (SubmissionStatus Status, string? SubmissionId, DateTime? SubmittedAt) submissionData;

                if (latestSubmission == null)
                {
                    submissionData = (SubmissionStatus.NotSubmitted, null, null);
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

                    submissionData = (status, latestSubmission.Id, latestSubmission.SubmittedAt);
                }

                // Cache the result for 5 minutes
                var cacheKey = $"{SubmissionStatusCacheKey}{expectedReturnId}";
                _cache.Set(cacheKey, submissionData, TimeSpan.FromMinutes(1));

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
        public async Task<BulkSubmissionResultDTO> BulkSubmitByPeriodAsync(string periodId, string saccoId, string saccoType)
        {
            var result = new BulkSubmissionResultDTO();

            try
            {
                // Get all expected returns for the period that are in draft status
                var expectedReturns = await _context.ExpectedReturns
                    .Include(er => er.ReturnForm)
                    .Include(er => er.Period)
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

                // Get submission statuses for all expected returns
                var expectedReturnIds = expectedReturns.Select(er => er.Id).ToList();
                var submissionStatuses = await GetSubmissionStatusesAsync(expectedReturnIds);

                // Filter to only draft submissions
                var draftReturns = expectedReturns.Where(er =>
                {
                    var status = submissionStatuses.GetValueOrDefault(er.Id).Status;
                    return status == SubmissionStatus.Draft;
                }).ToList();

                if (!draftReturns.Any())
                {
                    result.Success = false;
                    result.Message = "No draft returns found for bulk submission.";
                    return result;
                }

                result.TotalForms = draftReturns.Count;
                var details = new List<BulkSubmissionDetailDTO>();

                foreach (var expectedReturn in draftReturns)
                {
                    var detail = new BulkSubmissionDetailDTO
                    {
                        ExpectedReturnId = expectedReturn.Id,
                        FormName = expectedReturn.ReturnForm.FormName,
                        FormCode = expectedReturn.ReturnForm.Code,
                        Success = false,
                        Message = "",
                        Status = SubmissionStatus.Pending
                    };

                    try
                    {
                        // Get the latest submission for this expected return
                        var latestSubmission = expectedReturn.ReturnSubmissions
                            .OrderByDescending(rs => rs.SubmittedAt)
                            .FirstOrDefault();

                        if (latestSubmission == null)
                        {
                            detail.Message = "No draft submission found.";
                            details.Add(detail);
                            result.FailedSubmissions++;
                            continue;
                        }

                        // Update the submission status to Submitted
                        latestSubmission.Status = ExpectedStatus.Filed.ToString();
                        latestSubmission.SubmittedAt = DateTime.Now;

                        // Update the expected return status
                        expectedReturn.Status = ExpectedStatus.Filed;

                        detail.Success = true;
                        detail.Message = "Successfully submitted.";
                        detail.SubmissionId = latestSubmission.Id;
                        detail.Status = SubmissionStatus.Submitted;
                        result.SuccessfullySubmitted++;

                        details.Add(detail);
                    }
                    catch (Exception ex)
                    {
                        detail.Message = $"Error submitting form: {ex.Message}";
                        details.Add(detail);
                        result.FailedSubmissions++;
                        _logger.LogError(ex, "Error submitting form {FormName} for expected return {ExpectedReturnId}",
                            expectedReturn.ReturnForm.FormName, expectedReturn.Id);
                    }
                }

                // Save all changes
                await _context.SaveChangesAsync();

                result.Success = result.FailedSubmissions == 0;
                result.Message = result.Success
                    ? $"Successfully submitted {result.SuccessfullySubmitted} forms."
                    : $"Submitted {result.SuccessfullySubmitted} forms with {result.FailedSubmissions} failures.";
                result.Details = details;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk submission for period {PeriodId}", periodId);
                result.Success = false;
                result.Message = $"An error occurred during bulk submission: {ex.Message}";
                return result;
            }
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
    }
}
