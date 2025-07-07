using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns_Analysis;
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

        public async Task<CamelsRatingsDTO> CalculateAnalysisAsync(string returnId, string saccoType)
        {
            try
            {
                if (saccoType == Constants.SaccoType.DepositTaking.ToString())
                {
                    return await CalculateDepositTakingAnalysisAsync(returnId);
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


        private async Task<CamelsRatingsDTO> CalculateDepositTakingAnalysisAsync(string returnId)
        {
            try
            {
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
                /*foreach (var p in periods)
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
                                             .FirstOrDefaultAsync(m => m.ReturnId == p.Id)?? new ManagementReturn();

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
                    


                   var mgtRating  = AnalyzeManagement(managementReturn);
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
*/
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
        }

        private async Task<CamelsRatingsDTO> CalculateNwdtAnalysisAsync(string returnId)
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

              /*  foreach (var p in periods)
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
                    //var mgtRating = AnalyzeManagement(managementReturn);
                    //dto.ManagementRatingResults.Add(mgtRating);
                    //analysis.ManagementRating = AnalyzeManagement(managementReturn);
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
*/
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


        private string DetermineRiskLevel(int rating) => rating switch
        {
            1 or 2 => "Low Risk",
            3 => "Medium Risk",
            4 or 5 => "High Risk",
            _ => "Unknown"
        };


    }
}
