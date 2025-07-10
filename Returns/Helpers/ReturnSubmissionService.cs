using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Submission.DT;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using System;
using static Returns.Helpers.Constants;

namespace Returns.Helpers
{
    public class ReturnSubmissionService : IReturnSubmissionService
    {
        private readonly ReturnsDbContext _context;
        private readonly IExcelParser _excelParser;
        private readonly ILogger<ReturnSubmissionService> _logger;

        public ReturnSubmissionService(
            ReturnsDbContext context,
            IExcelParser excelParser,
            ILogger<ReturnSubmissionService> logger)
        {
            _context = context;
            _excelParser = excelParser;
            _logger = logger;
        }

        public async Task<IList<SubmissionResultDto>> UploadDraftAsync(NewReturnDTO dto, string SaccoType, string SaccoId)
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

                    if (expected == null || expected.ReturnForm.SaccoTypeId != SaccoType)
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
                        SaccoId = SaccoId,
                        SubmittedAt = dto.SubmissionDate,
                        FileUrl = url,
                    };

                    await _context.ReturnSubmissions.AddAsync(submission);
                    await _context.SaveChangesAsync();
                    res.SubmissionId = submission.Id;

                    FormCategory Category = (FormCategory)expected.ReturnForm.Category;

                    // Parse the Excel file
                    var parse = await _excelParser.ParseAsync(item.formFile, Category, SaccoType);

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
