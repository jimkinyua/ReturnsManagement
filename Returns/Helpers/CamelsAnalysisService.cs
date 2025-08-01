using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns.Returns_Submission;
using Returns.DTOs.Returns_Analysis;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using SASRAXRBSS.Dto.Returns_Analysis;
using System.Text.Json;
using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.Helpers
{
    public class CamelsAnalysisService : ICamelsAnalysisService
    {
        private readonly ReturnsDbContext _context;

        public CamelsAnalysisService(ReturnsDbContext context)
        {
            _context = context;
        }

        public async Task<CamelsRatingsDTO> CalculateAnalysisAsync(string groupId, string periodId, string saccoId, string saccoType)
        {
            try
            {
                if (saccoType == Constants.SaccoType.DepositTaking.ToString())
                {
                    return await CalculateDepositTakingAnalysisAsync(groupId, periodId, saccoId);
                }

                if (saccoType == Constants.SaccoType.NWDT.ToString())
                {
                    return await CalculateNwdtAnalysisAsync(groupId, periodId, saccoId);
                }

                throw new ArgumentException($"Unsupported sacco type: {saccoType}", nameof(saccoType));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CamelsRatingsDTO> CalculateCurrentAnalysisAsync(string groupId, string periodId, string saccoId, string saccoType)
        {
            try
            {
                if (saccoType == Constants.SaccoType.DepositTaking.ToString())
                {
                    return await CalculateCurrentDepositTakingAnalysisAsync(groupId, periodId, saccoId);
                }

                if (saccoType == Constants.SaccoType.NWDT.ToString())
                {
                    return await CalculateCurrentNwdtAnalysisAsync(groupId, periodId, saccoId);
                }

                throw new ArgumentException($"Unsupported sacco type: {saccoType}", nameof(saccoType));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<RatingDefination> GetRatingDefinitionAsync(string groupId)
        {
            var def = await _context.RatingDefinations
                        .Include(rd => rd.RatingForms)
                        .AsNoTracking()
                        .SingleOrDefaultAsync(rd => rd.Id == groupId);

            if (def is null)
                throw new KeyNotFoundException($"Rating definition '{groupId}' not found.");

            return def;
        }

        private async Task<List<ReturnPeriods>> GetCurrentAndHistoryAsync(string periodId)
        {
            var current = await _context.ReturnPeriods.FindAsync(periodId);
            if (current == null) throw new KeyNotFoundException($"Return period '{periodId}' not found.");

            return await _context.ReturnPeriods
                .Where(p => p.FrequencyId == current.FrequencyId &&
                            p.StartDate <= current.StartDate)
                .OrderByDescending(p => p.StartDate)
                .Take(4)
                .ToListAsync();
        }

        private async Task<CamelsRatingsDTO> CalculateDepositTakingAnalysisAsync(string groupId, string periodId, string saccoId)
        {
            try
            {
                var dto = new CamelsRatingsDTO
                {
                    PeriodId = periodId,
                    SaccoId = saccoId,
                    RatingDefinitionId = groupId
                };

                var ratingDef = await GetRatingDefinitionAsync(groupId);
                var requiredCodes = ratingDef.RatingForms
                             .Select(rf => rf.FormCode)
                             .ToList();
                var periods = await GetCurrentAndHistoryAsync(periodId);
                List<List<DTRiskClassificationReturn>> riskHistory = new();   // keeps each period’s risks

                foreach (var period in periods)
                {
                    var submissions = await GetFiledSubmissionsAsync(period.Id, saccoId, requiredCodes);

                    var subsByCategory = submissions.Values
                          .Where(s => s.ExpectedReturn?.ReturnForm != null)
                          .ToDictionary(
                            s => (FormCategory)s.ExpectedReturn.ReturnForm.Category,
                            s => s);

                    ReturnSubmission? FindSubmission(FormCategory category)
                    {
                        foreach (var sub in submissions.Values)                          // submissions already includes ExpectedReturn & ReturnForm
                        {
                            var form = sub.ExpectedReturn?.ReturnForm;
                            if (form is null) continue;

                            if ((FormCategory)form.Category == category)                 // compare enum values
                                return sub;
                        }
                        return null;
                    }

                    var depositSub = FindSubmission(FormCategory.DepositReturn);
                    var riskGridSub = FindSubmission(FormCategory.RiskClassification);

                    // 2. Resolve slice pieces
                    var finPosSub = FindSubmission(FormCategory.FinancialPosition);
                    var incStmtSub = FindSubmission(FormCategory.StatementOfComprehensiveIncome);
                    var capAdeSub = FindSubmission(FormCategory.CapitalAdequacy);
                    var liqSub = FindSubmission(FormCategory.LiquidityStatement);
                    var depSub = FindSubmission(FormCategory.DepositReturn);
                    var riskSub = FindSubmission(FormCategory.RiskClassification);
                    var mgtSub = FindSubmission(FormCategory.Management);

                    DTFinancialPositionReturn balanceSheet;
                    DTComprehensiveIncomeReturn incomeStmt;
                    DTCapitalAdequacyReturn capital;
                    DTLiquidityReturn liquidity;
                    List<DepositReturn> deposits;
                    List<DTRiskClassificationReturn> risks;
                    ManagementReturn management;

                    balanceSheet = finPosSub is null
                    ? new DTFinancialPositionReturn()
                    : await FromSubmissionAsync<DTFinancialPositionReturn>(finPosSub)
                      ?? new DTFinancialPositionReturn();

                    incomeStmt = incStmtSub is null
                        ? new DTComprehensiveIncomeReturn()
                        : await FromSubmissionAsync<DTComprehensiveIncomeReturn>(incStmtSub)
                          ?? new DTComprehensiveIncomeReturn();

                    capital = capAdeSub is null
                        ? new DTCapitalAdequacyReturn()
                        : await FromSubmissionAsync<DTCapitalAdequacyReturn>(capAdeSub)
                          ?? new DTCapitalAdequacyReturn();

                    liquidity = liqSub is null
                        ? new DTLiquidityReturn()
                        : await FromSubmissionAsync<DTLiquidityReturn>(liqSub)
                          ?? new DTLiquidityReturn();

                    deposits = depSub is null
                        ? new List<DepositReturn>()
                        : await _context.DepositReturns
                              .Where(d => d.ReturnSubmissionId == depSub.Id)
                              .ToListAsync();
                    risks = riskSub is null
                    ? new List<DTRiskClassificationReturn>()
                    : await _context.DTRiskClassificationReturns
                          .Where(r => r.ReturnSubmissionId == riskSub.Id)
                          .ToListAsync();

                    management = mgtSub is null
                    ? new ManagementReturn()
                    : await FromSubmissionAsync<ManagementReturn>(mgtSub)
                      ?? new ManagementReturn();

                    var slice = new
                    {
                        BalanceSheet = balanceSheet,
                        IncomeStmt = incomeStmt,
                        Capital = capital,
                        Liquidity = liquidity,
                        Deposits = deposits,
                        Risks = risks,
                        Management = management
                    };

                    var capData = new CapitalAnalysisData
                    {
                        CoreCapital = slice.Capital.CoreCapital,
                        CoreCapitalToAssetsRatio = slice.Capital.CoreCapitalToAssetsRatio,
                        InstitutionalCapitalRatio = slice.Capital.InstitutionalCapitalToAssetsRatio,
                        CoreCapitalToDepositsRatio = slice.Capital.CoreCapitalToDepositsRatio,
                        AdjustedCCARatio = CalculateAdjustedCCA(slice.Capital, slice.BalanceSheet)
                    };

                    var currentRisks = slice.Risks;
                    var previousRisks = riskHistory.Any()
                                         ? riskHistory.Last()         // risks from the last period processed
                                         : new List<DTRiskClassificationReturn>();

                    var capResult = await AnalyzeCapitalWithDetails(capData);
                    var aqResult = await AnalyzeAssetQuality(currentRisks, previousRisks);
                    var mgtResult = AnalyzeManagement(slice.Management);
                    var ernResult = await AnalyzeEarnings(slice.IncomeStmt, slice.BalanceSheet);
                    var liqResult = await AnalyzeLiquidity(slice.BalanceSheet);
                    var soaResult = await AnalyzeStructureOfAssets(slice.BalanceSheet);

                    // —— stamp the period label & add to DTO ——
                    var label = period.StartDate.ToString("yyyy-MM-dd");
                    capResult.Period = label;
                    aqResult.Period = label;
                    ernResult.Period = label;
                    liqResult.Period = label;
                    soaResult.Period = label;

                    dto.CapitalAnalysisResults.Add(capResult);
                    dto.AssetQualityRatingResults.Add(aqResult);
                    dto.ManagementRatingResults.Add(mgtResult);
                    dto.EarningsRatingResults.Add(ernResult);
                    dto.LiquidityRatingResults.Add(liqResult);
                    dto.StructureOfAssetsRatingResults.Add(soaResult);

                    // remember for next iteration
                    riskHistory.Add(currentRisks);
                }
                // ── 2.  Pad lists to ≥ 4 entries (current + 3 history) ───────────────────
                while (dto.CapitalAnalysisResults.Count < 4) dto.CapitalAnalysisResults.Add(new() { FinalRating = 0, Period = "N/A" });
                while (dto.AssetQualityRatingResults.Count < 4) dto.AssetQualityRatingResults.Add(new() { FinalRating = 0, Period = "N/A" });
                while (dto.EarningsRatingResults.Count < 4) dto.EarningsRatingResults.Add(new() { FinalRating = 0, Period = "N/A" });
                while (dto.LiquidityRatingResults.Count < 4) dto.LiquidityRatingResults.Add(new() { FinalRating = 0, Period = "N/A" });
                while (dto.StructureOfAssetsRatingResults.Count < 4) dto.StructureOfAssetsRatingResults.Add(new() { FinalRating = 0, Period = "N/A" });

                // ── 3.  Copy headline ratings from *current* period (index 0) ────────────
                dto.CapitalRating = dto.CapitalAnalysisResults[0].FinalRating;
                dto.AssetQualityRating = dto.AssetQualityRatingResults[0].FinalRating;
                dto.ManagementRating = dto.ManagementRatingResults[0].MRating;
                dto.EarningsRating = dto.EarningsRatingResults[0].FinalRating;
                dto.LiquidityRating = dto.LiquidityRatingResults[0].FinalRating;

                dto.OverallRating = CalculateOverallRating(
                                        dto.CapitalRating,
                                        dto.AssetQualityRating,
                                        dto.EarningsRating,
                                        dto.LiquidityRating,
                                        dto.ManagementRating);

                dto.Average = (dto.CapitalRating + dto.AssetQualityRating + dto.EarningsRating +
                                  dto.LiquidityRating + dto.ManagementRating) / 5.0m;
                dto.RiskLevel = DetermineRiskLevel(dto.OverallRating);

                return dto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<T?> FromSubmissionAsync<T>(ReturnSubmission sub) where T : class
        {
            return await _context.Set<T>()
                .FirstOrDefaultAsync(e => EF.Property<string>(e, "ReturnSubmissionId") == sub.Id);
        }


        private async Task<Dictionary<string, ReturnSubmission>> GetFiledSubmissionsAsync(string periodId, string saccoId, IReadOnlyCollection<string> requiredFormCodes)
        {
            // One round‑trip: filter by period + sacco + code + status, then group.
            return await _context.ReturnSubmissions
                // restrict to this SACCO and only active rows
                .Where(rs => rs.SaccoId == saccoId && rs.IsActive)

                // need the joined tables for filtering and later enum lookup
                .Include(rs => rs.ExpectedReturn)
                    .ThenInclude(er => er.ReturnForm)

                // filter by period and required form codes
                .Where(rs =>
                    rs.ExpectedReturn.PeriodId == periodId
                    //&& requiredFormCodes.Contains(rs.ExpectedReturn.ReturnForm.Code)
                    && rs.Status == SubmissionStatus.Submitted.ToString()
                    )

                // latest per ExpectedReturn
                .GroupBy(rs => rs.ExpectedReturnId)
                .Select(g => g.OrderByDescending(x => x.SubmittedAt).First())

                // dictionary keyed by ExpectedReturnId
                .ToDictionaryAsync(rs => rs.ExpectedReturnId);
        }
        private static ReturnSubmission? FindSubmissionByFormCode(IEnumerable<ReturnSubmission> submissions, string formCode)
        {
            foreach (var submission in submissions)
            {
                var codeOnSubmission = submission.ExpectedReturn?.ReturnForm?.Code;

                if (string.Equals(codeOnSubmission, formCode, StringComparison.OrdinalIgnoreCase))
                {
                    return submission;   // match found — stop here
                }
            }

            return null;  // none matched
        }

        private async Task<CamelsRatingsDTO> CalculateNwdtAnalysisAsync(string groupId, string periodId, string saccoId)
        {
            try
            {
                var dto = new CamelsRatingsDTO
                {
                    PeriodId = periodId,
                    SaccoId = saccoId,
                    RatingDefinitionId = groupId
                };

                var ratingDef = await GetRatingDefinitionAsync(groupId);
                var requiredCodes = ratingDef.RatingForms
                             .Select(rf => rf.FormCode)
                             .ToList();
                var periods = await GetCurrentAndHistoryAsync(periodId);
                List<List<NWDTRiskClassificationReturn>> riskHistory = new();   // keeps each period's NWDT risks

                foreach (var period in periods)
                {
                    var submissions = await GetFiledSubmissionsAsync(period.Id, saccoId, requiredCodes);

                    var subsByCategory = submissions.Values
                          .Where(s => s.ExpectedReturn?.ReturnForm != null)
                          .ToDictionary(
                            s => (FormCategory)s.ExpectedReturn.ReturnForm.Category,
                            s => s);

                    ReturnSubmission? FindSubmission(FormCategory category)
                    {
                        foreach (var sub in submissions.Values)                          // submissions already includes ExpectedReturn & ReturnForm
                        {
                            var form = sub.ExpectedReturn?.ReturnForm;
                            if (form is null) continue;

                            if ((FormCategory)form.Category == category)                 // compare enum values
                                return sub;
                        }
                        return null;
                    }

                    // 2. Resolve slice pieces
                    var finPosSub = FindSubmission(FormCategory.FinancialPosition);
                    var incStmtSub = FindSubmission(FormCategory.StatementOfComprehensiveIncome);
                    var capAdeSub = FindSubmission(FormCategory.CapitalAdequacy);
                    var liqSub = FindSubmission(FormCategory.LiquidityStatement);
                    var depSub = FindSubmission(FormCategory.DepositReturn);
                    var riskSub = FindSubmission(FormCategory.RiskClassification);
                    var mgtSub = FindSubmission(FormCategory.Management);
                    var investSub = FindSubmission(FormCategory.InvestmentReturn);

                    NWDTFinancialPositionReturn balanceSheet;
                    NWDTComprehensiveIncomeReturn incomeStmt;
                    NWDTCapitalAdequacyReturn capital;
                    NWDTLiquidityReturn liquidity;
                    List<NWDTDepositReturn> deposits;
                    List<NWDTRiskClassificationReturn> risks;
                    NWDTInvestmentReturn investment;
                    ManagementReturn management;

                    balanceSheet = finPosSub is null
                    ? new NWDTFinancialPositionReturn()
                    : await FromSubmissionAsync<NWDTFinancialPositionReturn>(finPosSub)
                      ?? new NWDTFinancialPositionReturn();

                    incomeStmt = incStmtSub is null
                        ? new NWDTComprehensiveIncomeReturn()
                        : await FromSubmissionAsync<NWDTComprehensiveIncomeReturn>(incStmtSub)
                          ?? new NWDTComprehensiveIncomeReturn();

                    capital = capAdeSub is null
                        ? new NWDTCapitalAdequacyReturn()
                        : await FromSubmissionAsync<NWDTCapitalAdequacyReturn>(capAdeSub)
                          ?? new NWDTCapitalAdequacyReturn();

                    liquidity = liqSub is null
                        ? new NWDTLiquidityReturn()
                        : await FromSubmissionAsync<NWDTLiquidityReturn>(liqSub)
                          ?? new NWDTLiquidityReturn();

                    deposits = depSub is null
                        ? new List<NWDTDepositReturn>()
                        : await _context.NWDTDepositReturns
                              .Where(d => d.ReturnSubmissionId == depSub.Id)
                              .ToListAsync();

                    risks = riskSub is null
                    ? new List<NWDTRiskClassificationReturn>()
                    : await _context.NWDTRiskClassificationReturns
                          .Where(r => r.ReturnSubmissionId == riskSub.Id)
                          .ToListAsync();

                    investment = investSub is null
                    ? new NWDTInvestmentReturn()
                    : await FromSubmissionAsync<NWDTInvestmentReturn>(investSub)
                      ?? new NWDTInvestmentReturn();

                    management = mgtSub is null
                    ? new ManagementReturn()
                    : await FromSubmissionAsync<ManagementReturn>(mgtSub)
                      ?? new ManagementReturn();

                    var slice = new
                    {
                        BalanceSheet = balanceSheet,
                        IncomeStmt = incomeStmt,
                        Capital = capital,
                        Liquidity = liquidity,
                        Deposits = deposits,
                        Risks = risks,
                        Investment = investment,
                        Management = management
                    };

                    var capData = new CapitalAnalysisData
                    {
                        CoreCapital = slice.Capital.CoreCapital,
                        CoreCapitalToAssetsRatio = slice.Capital.CoreCapitalToAssetsRatio,
                        CoreCapitalToDepositsRatio = slice.Capital.CoreCapitalToDepositsRatio,
                        AdjustedCCARatio = ReturnAnalysisHelper.CalculateNwdtAdjustedCCA(slice.Capital, slice.BalanceSheet)
                    };

                    var currentRisks = slice.Risks;
                    var previousRisks = riskHistory.Any()
                                         ? riskHistory.Last()         // risks from the last period processed
                                         : new List<NWDTRiskClassificationReturn>();

                    var capResult = await AnalyzeCapitalWithDetails(capData);
                    var aqResult = await ReturnAnalysisHelper.AnalyzeNwdtAssetQuality(currentRisks, previousRisks);
                    var mgtResult = AnalyzeManagement(slice.Management);
                    var ernResult = await ReturnAnalysisHelper.AnalyzeNwdtEarnings(slice.IncomeStmt, slice.BalanceSheet);
                    var liqResult = await ReturnAnalysisHelper.AnalyzeNwdtLiquidity(slice.BalanceSheet);
                    var soaResult = await ReturnAnalysisHelper.AnalyzeNwdtStructureOfAssets(slice.BalanceSheet);

                    // —— stamp the period label & add to DTO ——
                    var label = period.StartDate.ToString("yyyy-MM-dd");
                    capResult.Period = label;
                    aqResult.Period = label;
                    ernResult.Period = label;
                    liqResult.Period = label;
                    soaResult.Period = label;

                    dto.CapitalAnalysisResults.Add(capResult);
                    dto.AssetQualityRatingResults.Add(aqResult);
                    dto.ManagementRatingResults.Add(mgtResult);
                    dto.EarningsRatingResults.Add(ernResult);
                    dto.LiquidityRatingResults.Add(liqResult);
                    dto.StructureOfAssetsRatingResults.Add(soaResult);

                    // remember for next iteration
                    riskHistory.Add(currentRisks);
                }
                // ── 2.  Pad lists to ≥ 4 entries (current + 3 history) ───────────────────
                while (dto.CapitalAnalysisResults.Count < 4) dto.CapitalAnalysisResults.Add(new() { FinalRating = 0, Period = "N/A" });
                while (dto.AssetQualityRatingResults.Count < 4) dto.AssetQualityRatingResults.Add(new() { FinalRating = 0, Period = "N/A" });
                while (dto.EarningsRatingResults.Count < 4) dto.EarningsRatingResults.Add(new() { FinalRating = 0, Period = "N/A" });
                while (dto.LiquidityRatingResults.Count < 4) dto.LiquidityRatingResults.Add(new() { FinalRating = 0, Period = "N/A" });
                while (dto.StructureOfAssetsRatingResults.Count < 4) dto.StructureOfAssetsRatingResults.Add(new() { FinalRating = 0, Period = "N/A" });

                // ── 3.  Copy headline ratings from *current* period (index 0) ────────────
                dto.CapitalRating = dto.CapitalAnalysisResults[0].FinalRating;
                dto.AssetQualityRating = dto.AssetQualityRatingResults[0].FinalRating;
                dto.ManagementRating = dto.ManagementRatingResults[0].MRating;
                dto.EarningsRating = dto.EarningsRatingResults[0].FinalRating;
                dto.LiquidityRating = dto.LiquidityRatingResults[0].FinalRating;

                dto.OverallRating = CalculateOverallRating(
                                        dto.CapitalRating,
                                        dto.AssetQualityRating,
                                        dto.EarningsRating,
                                        dto.LiquidityRating,
                                        dto.ManagementRating);

                dto.Average = (dto.CapitalRating + dto.AssetQualityRating + dto.EarningsRating +
                                  dto.LiquidityRating + dto.ManagementRating) / 5.0m;
                dto.RiskLevel = DetermineRiskLevel(dto.OverallRating);

                return dto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        private string DetermineRiskLevel(int rating) => rating switch
        {
            1 or 2 => "Low Risk",
            3 => "Medium Risk",
            4 or 5 => "High Risk",
            _ => "Unknown"
        };

        public async Task<CamelsRatingsDTO> CalculateCurrentDepositTakingAnalysisAsync(string groupId, string periodId, string saccoId)
        {
            try
            {
                var dto = new CamelsRatingsDTO
                {
                    PeriodId = periodId,
                    SaccoId = saccoId,
                    RatingDefinitionId = groupId
                };

                var ratingDef = await GetRatingDefinitionAsync(groupId);
                var requiredCodes = ratingDef.RatingForms
                             .Select(rf => rf.FormCode)
                             .ToList();

                // Fetch submissions only for the current period
                var submissions = await GetFiledSubmissionsAsync(periodId, saccoId, requiredCodes);

                var subsByCategory = submissions.Values
                      .Where(s => s.ExpectedReturn?.ReturnForm != null)
                      .ToDictionary(
                        s => (FormCategory)s.ExpectedReturn.ReturnForm.Category,
                        s => s);

                ReturnSubmission? FindSubmission(FormCategory category)
                {
                    foreach (var sub in submissions.Values)
                    {
                        var form = sub.ExpectedReturn?.ReturnForm;
                        if (form is null) continue;

                        if ((FormCategory)form.Category == category)
                            return sub;
                    }
                    return null;
                }

                var depositSub = FindSubmission(FormCategory.DepositReturn);
                var riskGridSub = FindSubmission(FormCategory.RiskClassification);

                // Resolve slice pieces
                var finPosSub = FindSubmission(FormCategory.FinancialPosition);
                var incStmtSub = FindSubmission(FormCategory.StatementOfComprehensiveIncome);
                var capAdeSub = FindSubmission(FormCategory.CapitalAdequacy);
                var liqSub = FindSubmission(FormCategory.LiquidityStatement);
                var depSub = FindSubmission(FormCategory.DepositReturn);
                var riskSub = FindSubmission(FormCategory.RiskClassification);
                var mgtSub = FindSubmission(FormCategory.Management);

                DTFinancialPositionReturn balanceSheet = finPosSub is null
                    ? new DTFinancialPositionReturn()
                    : await FromSubmissionAsync<DTFinancialPositionReturn>(finPosSub)
                      ?? new DTFinancialPositionReturn();

                DTComprehensiveIncomeReturn incomeStmt = incStmtSub is null
                    ? new DTComprehensiveIncomeReturn()
                    : await FromSubmissionAsync<DTComprehensiveIncomeReturn>(incStmtSub)
                      ?? new DTComprehensiveIncomeReturn();

                DTCapitalAdequacyReturn capital = capAdeSub is null
                    ? new DTCapitalAdequacyReturn()
                    : await FromSubmissionAsync<DTCapitalAdequacyReturn>(capAdeSub)
                      ?? new DTCapitalAdequacyReturn();

                DTLiquidityReturn liquidity = liqSub is null
                    ? new DTLiquidityReturn()
                    : await FromSubmissionAsync<DTLiquidityReturn>(liqSub)
                      ?? new DTLiquidityReturn();

                List<DepositReturn> deposits = depSub is null
                    ? new List<DepositReturn>()
                    : await _context.DepositReturns
                          .Where(d => d.ReturnSubmissionId == depSub.Id)
                          .ToListAsync();

                List<DTRiskClassificationReturn> risks = riskSub is null
                    ? new List<DTRiskClassificationReturn>()
                    : await _context.DTRiskClassificationReturns
                          .Where(r => r.ReturnSubmissionId == riskSub.Id)
                          .ToListAsync();

                ManagementReturn management = mgtSub is null
                    ? new ManagementReturn()
                    : await FromSubmissionAsync<ManagementReturn>(mgtSub)
                      ?? new ManagementReturn();

                var slice = new
                {
                    BalanceSheet = balanceSheet,
                    IncomeStmt = incomeStmt,
                    Capital = capital,
                    Liquidity = liquidity,
                    Deposits = deposits,
                    Risks = risks,
                    Management = management
                };

                var capData = new CapitalAnalysisData
                {
                    CoreCapital = slice.Capital.CoreCapital,
                    CoreCapitalToAssetsRatio = slice.Capital.CoreCapitalToAssetsRatio,
                    InstitutionalCapitalRatio = slice.Capital.InstitutionalCapitalToAssetsRatio,
                    CoreCapitalToDepositsRatio = slice.Capital.CoreCapitalToDepositsRatio,
                    AdjustedCCARatio = CalculateAdjustedCCA(slice.Capital, slice.BalanceSheet)
                };

                var currentRisks = slice.Risks;
                // No history: use empty list for previous risks
                var previousRisks = new List<DTRiskClassificationReturn>();

                var capResult = await AnalyzeCapitalWithDetails(capData);
                var aqResult = await AnalyzeAssetQuality(currentRisks, previousRisks);
                var mgtResult = AnalyzeManagement(slice.Management);
                var ernResult = await AnalyzeEarnings(slice.IncomeStmt, slice.BalanceSheet);
                var liqResult = await AnalyzeLiquidity(slice.BalanceSheet);
                var soaResult = await AnalyzeStructureOfAssets(slice.BalanceSheet);

                // Stamp the period label
                var label = (await _context.ReturnPeriods.FindAsync(periodId))?.StartDate.ToString("yyyy-MM-dd") ?? "Current";
                capResult.Period = label;
                aqResult.Period = label;
                ernResult.Period = label;
                liqResult.Period = label;
                soaResult.Period = label;

                dto.CapitalAnalysisResults.Add(capResult);
                dto.AssetQualityRatingResults.Add(aqResult);
                dto.ManagementRatingResults.Add(mgtResult);
                dto.EarningsRatingResults.Add(ernResult);
                dto.LiquidityRatingResults.Add(liqResult);
                dto.StructureOfAssetsRatingResults.Add(soaResult);

                // Copy headline ratings from current results
                dto.CapitalRating = capResult.FinalRating;
                dto.AssetQualityRating = aqResult.FinalRating;
                dto.ManagementRating = mgtResult.MRating;
                dto.EarningsRating = ernResult.FinalRating;
                dto.LiquidityRating = liqResult.FinalRating;

                dto.OverallRating = CalculateOverallRating(
                                        dto.CapitalRating,
                                        dto.AssetQualityRating,
                                        dto.EarningsRating,
                                        dto.LiquidityRating,
                                        dto.ManagementRating);

                dto.Average = (dto.CapitalRating + dto.AssetQualityRating + dto.EarningsRating +
                                  dto.LiquidityRating + dto.ManagementRating) / 5.0m;
                dto.RiskLevel = DetermineRiskLevel(dto.OverallRating);

                // Prepare calculation details as JSON
                var calculationDetails = new
                {
                    CapitalResult = capResult,
                    AssetQualityResult = aqResult,
                    ManagementResult = mgtResult,
                    EarningsResult = ernResult,
                    LiquidityResult = liqResult,
                    StructureOfAssetsResult = soaResult
                };

                var jsonDetails = JsonSerializer.Serialize(calculationDetails);

                // Save the rating to the database
                var ratingEntity = new CAELSRating
                {
                    PeriodId = dto.PeriodId,
                    SaccoId = dto.SaccoId,
                    RatingDefinitionId = dto.RatingDefinitionId,
                    CapitalRating = dto.CapitalRating,
                    AssetQualityRating = dto.AssetQualityRating,
                    ManagementRating = dto.ManagementRating,
                    EarningsRating = dto.EarningsRating,
                    LiquidityRating = dto.LiquidityRating,
                    OverallRating = dto.OverallRating,
                    AverageRating = dto.Average,
                    RiskLevel = dto.RiskLevel,
                    CalculationDetails = jsonDetails,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CAELSRatings.Add(ratingEntity);
                await _context.SaveChangesAsync();

                return dto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // New method for current-period-only NWDT analysis with saving (mirrors the DepositTaking version but adapted for NWDT models and helpers)
        public async Task<CamelsRatingsDTO> CalculateCurrentNwdtAnalysisAsync(string groupId, string periodId, string saccoId)
        {
            try
            {
                var dto = new CamelsRatingsDTO
                {
                    PeriodId = periodId,
                    SaccoId = saccoId,
                    RatingDefinitionId = groupId
                };

                var ratingDef = await GetRatingDefinitionAsync(groupId);
                var requiredCodes = ratingDef.RatingForms
                             .Select(rf => rf.FormCode)
                             .ToList();

                // Fetch submissions only for the current period
                var submissions = await GetFiledSubmissionsAsync(periodId, saccoId, requiredCodes);

                var subsByCategory = submissions.Values
                      .Where(s => s.ExpectedReturn?.ReturnForm != null)
                      .ToDictionary(
                        s => (FormCategory)s.ExpectedReturn.ReturnForm.Category,
                        s => s);

                ReturnSubmission? FindSubmission(FormCategory category)
                {
                    foreach (var sub in submissions.Values)
                    {
                        var form = sub.ExpectedReturn?.ReturnForm;
                        if (form is null) continue;

                        if ((FormCategory)form.Category == category)
                            return sub;
                    }
                    return null;
                }

                // Resolve slice pieces
                var finPosSub = FindSubmission(FormCategory.FinancialPosition);
                var incStmtSub = FindSubmission(FormCategory.StatementOfComprehensiveIncome);
                var capAdeSub = FindSubmission(FormCategory.CapitalAdequacy);
                var liqSub = FindSubmission(FormCategory.LiquidityStatement);
                var depSub = FindSubmission(FormCategory.DepositReturn);
                var riskSub = FindSubmission(FormCategory.RiskClassification);
                var mgtSub = FindSubmission(FormCategory.Management);
                var investSub = FindSubmission(FormCategory.InvestmentReturn);

                NWDTFinancialPositionReturn balanceSheet = finPosSub is null
                    ? new NWDTFinancialPositionReturn()
                    : await FromSubmissionAsync<NWDTFinancialPositionReturn>(finPosSub)
                      ?? new NWDTFinancialPositionReturn();

                NWDTComprehensiveIncomeReturn incomeStmt = incStmtSub is null
                    ? new NWDTComprehensiveIncomeReturn()
                    : await FromSubmissionAsync<NWDTComprehensiveIncomeReturn>(incStmtSub)
                      ?? new NWDTComprehensiveIncomeReturn();

                NWDTCapitalAdequacyReturn capital = capAdeSub is null
                    ? new NWDTCapitalAdequacyReturn()
                    : await FromSubmissionAsync<NWDTCapitalAdequacyReturn>(capAdeSub)
                      ?? new NWDTCapitalAdequacyReturn();

                NWDTLiquidityReturn liquidity = liqSub is null
                    ? new NWDTLiquidityReturn()
                    : await FromSubmissionAsync<NWDTLiquidityReturn>(liqSub)
                      ?? new NWDTLiquidityReturn();

                List<NWDTDepositReturn> deposits = depSub is null
                    ? new List<NWDTDepositReturn>()
                    : await _context.NWDTDepositReturns
                          .Where(d => d.ReturnSubmissionId == depSub.Id)
                          .ToListAsync();

                List<NWDTRiskClassificationReturn> risks = riskSub is null
                    ? new List<NWDTRiskClassificationReturn>()
                    : await _context.NWDTRiskClassificationReturns
                          .Where(r => r.ReturnSubmissionId == riskSub.Id)
                          .ToListAsync();

                NWDTInvestmentReturn investment = investSub is null
                    ? new NWDTInvestmentReturn()
                    : await FromSubmissionAsync<NWDTInvestmentReturn>(investSub)
                      ?? new NWDTInvestmentReturn();

                ManagementReturn management = mgtSub is null
                    ? new ManagementReturn()
                    : await FromSubmissionAsync<ManagementReturn>(mgtSub)
                      ?? new ManagementReturn();

                var slice = new
                {
                    BalanceSheet = balanceSheet,
                    IncomeStmt = incomeStmt,
                    Capital = capital,
                    Liquidity = liquidity,
                    Deposits = deposits,
                    Risks = risks,
                    Investment = investment,
                    Management = management
                };

                var capData = new CapitalAnalysisData
                {
                    CoreCapital = slice.Capital.CoreCapital,
                    CoreCapitalToAssetsRatio = slice.Capital.CoreCapitalToAssetsRatio,
                    CoreCapitalToDepositsRatio = slice.Capital.CoreCapitalToDepositsRatio,
                    AdjustedCCARatio = ReturnAnalysisHelper.CalculateNwdtAdjustedCCA(slice.Capital, slice.BalanceSheet)
                };

                var currentRisks = slice.Risks;
                // No history: use empty list for previous risks
                var previousRisks = new List<NWDTRiskClassificationReturn>();

                var capResult = await AnalyzeCapitalWithDetails(capData);
                var aqResult = await ReturnAnalysisHelper.AnalyzeNwdtAssetQuality(currentRisks, previousRisks);
                var mgtResult = AnalyzeManagement(slice.Management);
                var ernResult = await ReturnAnalysisHelper.AnalyzeNwdtEarnings(slice.IncomeStmt, slice.BalanceSheet);
                var liqResult = await ReturnAnalysisHelper.AnalyzeNwdtLiquidity(slice.BalanceSheet);
                var soaResult = await ReturnAnalysisHelper.AnalyzeNwdtStructureOfAssets(slice.BalanceSheet);

                // Stamp the period label
                var label = (await _context.ReturnPeriods.FindAsync(periodId))?.StartDate.ToString("yyyy-MM-dd") ?? "Current";
                capResult.Period = label;
                aqResult.Period = label;
                ernResult.Period = label;
                liqResult.Period = label;
                soaResult.Period = label;

                dto.CapitalAnalysisResults.Add(capResult);
                dto.AssetQualityRatingResults.Add(aqResult);
                dto.ManagementRatingResults.Add(mgtResult);
                dto.EarningsRatingResults.Add(ernResult);
                dto.LiquidityRatingResults.Add(liqResult);
                dto.StructureOfAssetsRatingResults.Add(soaResult);

                // Copy headline ratings from current results
                dto.CapitalRating = capResult.FinalRating;
                dto.AssetQualityRating = aqResult.FinalRating;
                dto.ManagementRating = mgtResult.MRating;
                dto.EarningsRating = ernResult.FinalRating;
                dto.LiquidityRating = liqResult.FinalRating;

                dto.OverallRating = CalculateOverallRating(
                                        dto.CapitalRating,
                                        dto.AssetQualityRating,
                                        dto.EarningsRating,
                                        dto.LiquidityRating,
                                        dto.ManagementRating);

                dto.Average = (dto.CapitalRating + dto.AssetQualityRating + dto.EarningsRating +
                                  dto.LiquidityRating + dto.ManagementRating) / 5.0m;
                dto.RiskLevel = DetermineRiskLevel(dto.OverallRating);

                // Prepare calculation details as JSON
                var calculationDetails = new
                {
                    CapitalResult = capResult,
                    AssetQualityResult = aqResult,
                    ManagementResult = mgtResult,
                    EarningsResult = ernResult,
                    LiquidityResult = liqResult,
                    StructureOfAssetsResult = soaResult
                };

                var jsonDetails = JsonSerializer.Serialize(calculationDetails);

                // Save the rating to the database
                var ratingEntity = new CAELSRating
                {
                    PeriodId = dto.PeriodId,
                    SaccoId = dto.SaccoId,
                    RatingDefinitionId = dto.RatingDefinitionId,
                    CapitalRating = dto.CapitalRating,
                    AssetQualityRating = dto.AssetQualityRating,
                    ManagementRating = dto.ManagementRating,
                    EarningsRating = dto.EarningsRating,
                    LiquidityRating = dto.LiquidityRating,
                    OverallRating = dto.OverallRating,
                    AverageRating = dto.Average,
                    RiskLevel = dto.RiskLevel,
                    CalculationDetails = jsonDetails,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CAELSRatings.Add(ratingEntity);
                await _context.SaveChangesAsync();

                return dto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}