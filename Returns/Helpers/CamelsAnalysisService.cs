using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns_Analysis;
using Returns.Helpers.Enums;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;
using SASRAXRBSS.Dto.Returns_Analysis;
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
                    return await CalculateNwdtAnalysisAsync(returnId);
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

       /* private async Task<CamelsRatingsDTO> CalculateDepositTakingAnalysisAsync(string groupId, string periodId, string saccoId)
        {
            try
            {
                var requiredCodes = await GetRequiredFormCodesAsync(groupId);
                var periods = await GetCurrentAndHistoryAsync(periodId);

                // 1. Load current return
                var current = await _context.Returns
                    .FirstOrDefaultAsync(r => r.Id == returnId &&
                                              r.SaccoType == Constants.SaccoType.DepositTaking.ToString());
                if (current == null)
                    throw new KeyNotFoundException($"Deposit-Taking return '{returnId}' not found.");

                // 2. Load the two most recent historical returns
                var history = await _context.Returns
                    .Where(r => r.SaccoType == Constants.SaccoType.DepositTaking.ToString() &&
                                r.CreatedAt < current.CreatedAt &&
                                r.Id != returnId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(2)
                    .ToListAsync();

                var periods = new[] { current }.Concat(history);

                // 3. Prepare the DTO
                var dto = new CamelsRatingsDTO { ReturnId = returnId };

                // 4. Loop each period and build analysis + DTO slices
                foreach (var p in periods)
                {
                    // —— fetch raw data (may be null) ——
                    var balanceSheet = await _context.DTFinancialPositionReturns
                                           .FirstOrDefaultAsync(x => x.ReturnId == p.Id)
                                       ?? new DTFinancialPositionReturn();

                    var incomeStmt = await _context.DTComprehensiveIncomeReturns
                                           .FirstOrDefaultAsync(x => x.ReturnId == p.Id)
                                       ?? new DTComprehensiveIncomeReturn();

                    var capitalReturn = await _context.DTCapitalAdequacyReturns
                                           .FirstOrDefaultAsync(x => x.ReturnId == p.Id)
                                       ?? new DTCapitalAdequacyReturn();

                    var liquidityRet = await _context.DTLiquidityReturns
                                           .FirstOrDefaultAsync(x => x.ReturnId == p.Id)
                                       ?? new DTLiquidityReturn();

                    var depositList = await _context.DepositReturns
                                           .Where(x => x.ReturnId == p.Id)
                                           .ToListAsync()
                                       ?? new List<DepositReturn>();

                    var riskList = await _context.DTRiskClassificationReturns
                                           .Where(x => x.ReturnId == p.Id)
                                           .ToListAsync()
                                       ?? new List<DTRiskClassificationReturn>();

                    var investRet = await _context.DTInvestmentReturns
                                           .FirstOrDefaultAsync(x => x.ReturnId == p.Id)
                                       ?? new DTInvestmentReturn();

                    var managementReturn = await _context.ManagementReturns
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(m => m.ReturnId == p.Id) ?? new ManagementReturn();

                    // —— persist analysis record ——
                    var analysis = new SaccoAnalysis
                    {
                        ReturnId = p.Id,
                        AnalysisDate = DateTime.UtcNow,
                        CoreCapital = capitalReturn.CoreCapital,
                        TotalAssets = capitalReturn.TotalAssets,
                        CoreCapitalToAssetsRatio = capitalReturn.CoreCapitalToAssetsRatio,
                        InstitutionalCapitalRatio = capitalReturn.InstitutionalCapitalToAssetsRatio,
                        NonPerformingLoans = ReturnAnalysisHelper.CalculateNonPerformingLoans(riskList),
                        TotalLoans = ReturnAnalysisHelper.CalculateTotalLoans(riskList),
                        NetIncome = incomeStmt.NetIncomeAfterTaxesAndDonations,
                        LiquidAssets = liquidityRet.NetLiquidAssets,
                        TotalDeposits = balanceSheet.TotalDepositLiabilities
                    };
                    analysis.OverallRating = CalculateOverallRating(analysis);
                    //analysis.ManagementRating = AnalyzeManagement(managementReturn);
                    _context.SaccoAnalysis.Add(analysis);
                    await _context.SaveChangesAsync();

                    // —— build DTO slices ——
                    var capData = new CapitalAnalysisData
                    {
                        CoreCapital = capitalReturn.CoreCapital,
                        CoreCapitalToAssetsRatio = capitalReturn.CoreCapitalToAssetsRatio,
                        InstitutionalCapitalRatio = capitalReturn.InstitutionalCapitalToAssetsRatio,
                        CoreCapitalToDepositsRatio = capitalReturn.CoreCapitalToDepositsRatio,
                        AdjustedCCARatio = CalculateAdjustedCCA(capitalReturn, balanceSheet)
                    };
                    var capRatings = await AnalyzeCapitalWithDetails(capData);
                    capRatings.Period = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.CapitalAnalysisResults.Add(capRatings);

                    var aqRatings = await AnalyzeAssetQuality(riskList, riskList);
                    aqRatings.Period = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.AssetQualityRatingResults.Add(aqRatings);



                    var mgtRating = AnalyzeManagement(managementReturn);
                    //mgtRating.ReturnPeriods = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.ManagementRatingResults.Add(mgtRating);

                    var earnRatings = await AnalyzeEarnings(incomeStmt, balanceSheet);
                    earnRatings.Period = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.EarningsRatingResults.Add(earnRatings);

                    var liqRatings = await AnalyzeLiquidity(balanceSheet);
                    liqRatings.Period = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.LiquidityRatingResults.Add(liqRatings);

                    var structRatings = await AnalyzeStructureOfAssets(balanceSheet);
                    structRatings.Period = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.StructureOfAssetsRatingResults.Add(structRatings);

                    // write back overall into our analysis record

                    await _context.SaveChangesAsync();
                }

                // 5. Pad each list to at least 3 entries
                while (dto.CapitalAnalysisResults.Count < 3) dto.CapitalAnalysisResults.Add(new CapitalAnalysisResult { FinalRating = 0, Period = "N/A" });
                while (dto.AssetQualityRatingResults.Count < 3) dto.AssetQualityRatingResults.Add(new AssetQualityRatingDetails { FinalRating = 0, Period = "N/A" });
                while (dto.EarningsRatingResults.Count < 3) dto.EarningsRatingResults.Add(new EarningsRatingDetails { FinalRating = 0, Period = "N/A" });
                while (dto.LiquidityRatingResults.Count < 3) dto.LiquidityRatingResults.Add(new LiquidityRatingDetails { FinalRating = 0, Period = "N/A" });
                while (dto.StructureOfAssetsRatingResults.Count < 3) dto.StructureOfAssetsRatingResults.Add(new StructureOfAssetsRatingDetails { FinalRating = 0, Period = "N/A" });

                // 6. Recompute final Overall & RiskLevel from the first (current) period
                dto.CapitalRating = dto.CapitalAnalysisResults[0].FinalRating;
                dto.AssetQualityRating = dto.AssetQualityRatingResults[0].FinalRating;
                dto.EarningsRating = dto.EarningsRatingResults[0].FinalRating;
                dto.LiquidityRating = dto.LiquidityRatingResults[0].FinalRating;
                dto.ManagementRating = dto.ManagementRatingResults[0].MRating;

                dto.OverallRating = CalculateOverallRating(
                                              dto.CapitalRating,
                                              dto.AssetQualityRating,
                                              dto.EarningsRating,
                                              dto.LiquidityRating,
                                              dto.ManagementRating
                                              );
                dto.Average = (dto.CapitalRating + dto.AssetQualityRating + dto.EarningsRating + dto.LiquidityRating + dto.ManagementRating) / 5.0m;
                dto.RiskLevel = DetermineRiskLevel(dto.OverallRating);

                return dto;
            }
            catch (Exception ex)
            {
                throw ;
            }
        }*/

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
                    var submissions = await GetFiledSubmissionsAsync(period.Id,saccoId, requiredCodes);

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


        private async Task<Dictionary<string, ReturnSubmission>> GetFiledSubmissionsAsync(string periodId, string saccoId,IReadOnlyCollection<string> requiredFormCodes)
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
                    rs.ExpectedReturn.PeriodId == periodId &&
                    requiredFormCodes.Contains(rs.ExpectedReturn.ReturnForm.Code)
                    //&& rs.Status == ExpectedStatus.Filed.ToString()
                    ) 

                // latest per ExpectedReturn
                .GroupBy(rs => rs.ExpectedReturnId)
                .Select(g => g.OrderByDescending(x => x.SubmittedAt).First())

                // dictionary keyed by ExpectedReturnId
                .ToDictionaryAsync(rs => rs.ExpectedReturnId);
        }
        private static ReturnSubmission? FindSubmissionByFormCode(IEnumerable<ReturnSubmission> submissions,string formCode)
        {
            foreach (var submission in submissions)
            {
                var codeOnSubmission = submission.ExpectedReturn?.ReturnForm?.Code;

                if (string.Equals(codeOnSubmission,formCode,StringComparison.OrdinalIgnoreCase))
                {
                    return submission;   // match found — stop here
                }
            }

            return null;  // none matched
        }



       /* private async Task<CamelsRatingsDTO> CalculateNwdtAnalysisAsync(string returnId)
        {
            try
            {
                // The same structure, but using NWDT tables & helpers.
                // — load current + history
                var current = await _context.Returns
                    .FirstOrDefaultAsync(r => r.Id == returnId && r.SaccoType == Constants.SaccoType.NWDT.ToString());
                if (current == null)
                    throw new KeyNotFoundException($"NWDT return '{returnId}' not found.");

                var history = await _context.Returns
                    .Where(r => r.SaccoType == Constants.SaccoType.NWDT.ToString()
                             && r.CreatedAt < current.CreatedAt
                             && r.Id != returnId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(2)
                    .ToListAsync();

                var periods = new[] { current }.Concat(history);
                var dto = new CamelsRatingsDTO { ReturnId = returnId };

                foreach (var p in periods)
                {
                    // fetch NWDT tables
                    var balanceSheet = await _context.NWDTFinancialPositionReturns
                                           .FirstOrDefaultAsync(x => x.ReturnId == p.Id)
                                       ?? new NWDTFinancialPositionReturn();

                    var incomeStmt = await _context.NWDTComprehensiveIncomeReturns
                                           .FirstOrDefaultAsync(x => x.ReturnId == p.Id)
                                       ?? new NWDTComprehensiveIncomeReturn();

                    var capitalReturn = await _context.NWDTCapitalAdequacyReturns
                                           .FirstOrDefaultAsync(x => x.ReturnId == p.Id)
                                       ?? new NWDTCapitalAdequacyReturn();

                    var liquidityRet = await _context.NDWTLiquidityReturns
                                           .FirstOrDefaultAsync(x => x.ReturnId == p.Id)
                                       ?? new NWDTLiquidityReturn();

                    var depositList = await _context.NWDTDepositReturns
                                           .Where(x => x.ReturnId == p.Id)
                                           .ToListAsync()
                                       ?? new List<NWDTDepositReturn>();

                    var riskList = await _context.NWDTRiskClassificationReturns
                                           .Where(x => x.ReturnId == p.Id)
                                           .ToListAsync()
                                       ?? new List<NWDTRiskClassificationReturn>();

                    var investRet = await _context.NWDTInvestmentReturns
                                           .FirstOrDefaultAsync(x => x.ReturnId == p.Id)
                                       ?? new NWDTInvestmentReturn();


                    var managementReturn = await _context.ManagementReturns
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(m => m.ReturnId == p.Id) ?? new ManagementReturn();


                    // persist analysis
                    var analysis = new SaccoAnalysis
                    {
                        ReturnId = p.Id,
                        AnalysisDate = DateTime.UtcNow,
                        CoreCapital = capitalReturn.CoreCapital,
                        TotalAssets = capitalReturn.TotalAssets,
                        CoreCapitalToAssetsRatio = capitalReturn.CoreCapitalToAssetsRatio,
                        NonPerformingLoans = ReturnAnalysisHelper.CalculateNwdtNonPerformingLoans(riskList),
                        TotalLoans = ReturnAnalysisHelper.CalculateNwdtTotalLoans(riskList),
                        NetIncome = incomeStmt.NetIncomeAfterTaxesAndDonations,
                        LiquidAssets = liquidityRet.NetLiquidAssets,
                        TotalDeposits = balanceSheet.TotalDepositLiabilities
                    };
                    analysis.OverallRating = CalculateOverallRating(analysis);
                    var mgtRating = AnalyzeManagement(managementReturn);
                    dto.ManagementRatingResults.Add(mgtRating);
                    analysis.ManagementRating = AnalyzeManagement(managementReturn);
                    _context.SaccoAnalysis.Add(analysis);
                    await _context.SaveChangesAsync();

                    // DTO slices
                    var capData = new CapitalAnalysisData
                    {
                        CoreCapital = capitalReturn.CoreCapital,
                        CoreCapitalToAssetsRatio = capitalReturn.CoreCapitalToAssetsRatio,
                        CoreCapitalToDepositsRatio = capitalReturn.CoreCapitalToDepositsRatio,
                        AdjustedCCARatio = CalculateNwdtAdjustedCCA(capitalReturn, balanceSheet)
                    };
                    var capRatings = await AnalyzeCapitalWithDetails(capData);
                    capRatings.Period = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.CapitalAnalysisResults.Add(capRatings);

                    var aqRatings = await AnalyzeNwdtAssetQuality(riskList, riskList);
                    aqRatings.Period = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.AssetQualityRatingResults.Add(aqRatings);

                    var mgrating = AnalyzeManagement(managementReturn);
                    dto.ManagementRatingResults.Add(mgrating);

                    var earnRatings = await AnalyzeNwdtEarnings(incomeStmt, balanceSheet);
                    earnRatings.Period = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.EarningsRatingResults.Add(earnRatings);

                    var liqRatings = await AnalyzeNwdtLiquidity(balanceSheet);
                    liqRatings.Period = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.LiquidityRatingResults.Add(liqRatings);

                    var structRatings = await AnalyzeNwdtStructureOfAssets(balanceSheet);
                    structRatings.Period = p.CreatedAt.ToString("yyyy-MM-dd");
                    dto.StructureOfAssetsRatingResults.Add(structRatings);


                    await _context.SaveChangesAsync();
                }

                // pad to 3 entries
                while (dto.CapitalAnalysisResults.Count < 3) dto.CapitalAnalysisResults.Add(new CapitalAnalysisResult { FinalRating = 0, Period = "N/A" });
                while (dto.AssetQualityRatingResults.Count < 3) dto.AssetQualityRatingResults.Add(new AssetQualityRatingDetails { FinalRating = 0, Period = "N/A" });
                while (dto.EarningsRatingResults.Count < 3) dto.EarningsRatingResults.Add(new EarningsRatingDetails { FinalRating = 0, Period = "N/A" });
                while (dto.LiquidityRatingResults.Count < 3) dto.LiquidityRatingResults.Add(new LiquidityRatingDetails { FinalRating = 0, Period = "N/A" });
                while (dto.StructureOfAssetsRatingResults.Count < 3) dto.StructureOfAssetsRatingResults.Add(new StructureOfAssetsRatingDetails { FinalRating = 0, Period = "N/A" });

                // final recompute
                dto.CapitalRating = dto.CapitalAnalysisResults[0].FinalRating;
                dto.AssetQualityRating = dto.AssetQualityRatingResults[0].FinalRating;
                dto.EarningsRating = dto.EarningsRatingResults[0].FinalRating;
                dto.LiquidityRating = dto.LiquidityRatingResults[0].FinalRating;
                dto.ManagementRating = dto.ManagementRatingResults[0].MRating;
                dto.OverallRating = CalculateOverallRating(
                                              dto.CapitalRating,
                                              dto.AssetQualityRating,
                                              dto.EarningsRating,
                                              dto.LiquidityRating,
                                              dto.ManagementRating
                                              );
                dto.RiskLevel = DetermineRiskLevel(dto.OverallRating);
                dto.Average = (dto.CapitalRating + dto.AssetQualityRating + dto.EarningsRating + dto.LiquidityRating + dto.ManagementRating) / 5.0m;

                return dto;
            }
            catch (Exception ex)
            {
                throw ;
            }
        }
*/

        private string DetermineRiskLevel(int rating) => rating switch
        {
            1 or 2 => "Low Risk",
            3 => "Medium Risk",
            4 or 5 => "High Risk",
            _ => "Unknown"
        };


    }
}
