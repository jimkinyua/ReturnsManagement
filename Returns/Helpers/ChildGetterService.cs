using Microsoft.EntityFrameworkCore;
using Returns.DTOs.Returns.Returns_Submission.DT;
using Returns.DTOs.Returns_Submission.DT;
using Returns.DTOs.Returns_Submission.NWDT;
using Returns.DTOs.Returns_Submission.Returns_Submission.DT;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Data;

namespace Returns.Helpers
{
    public class ChildGetterService : IReturnChild
    {
        private readonly ReturnsDbContext _context;
        public ChildGetterService(ReturnsDbContext dbContext)
        {
            _context = dbContext;
        }
        public async Task<object?> GetChildDetailsByFormType(string childId, string formType, string saccoType)
        {
            bool isNWDT = saccoType?.ToUpper() == "1";

            return formType switch
            {
                "CapitalAdequacy" => isNWDT
                    ? await GetNWDTCapitalAdequacyDT(childId)
                    : await GetCapitaAdequacyDT(childId),

                "Liquidity" => isNWDT
                    ? await GetNWDTLiquidityStatementDT(childId)
                    : await GetLiquidityStatementDT(childId),

                "DepositReturn" => isNWDT
                    ? await GetNWDTDepositReturnDT(childId)
                    : await GetDepositReturnDT(childId),

                "RiskClassification" => isNWDT
                    ? await GetNWDTRiskClassificationDT(childId)
                    : await GetRiskClassificationDT(childId),

                "Investment" => isNWDT
                    ? await GetNWDTInvestmentReturnDT(childId)
                    : await GetInvestmentReturnDT(childId),

                "FinancialPosition" => isNWDT
                    ? await GetNWDTFinancialPositionDT(childId)
                    : await GetFinancialPositionDT(childId),

                "Management" => isNWDT
                    ? await GetNWDTManagementReturnDT(childId)
                    : await GetManagementReturnDT(childId),

                "ComprehensiveIncome" => isNWDT
                    ? await GetNWDTComprehensiveIncomeDT(childId)
                    : await GetComprehensiveIncomeDT(childId),

                // For form types that might be shared or only exist in one type
                /*"SectoralLending" => await GetSectoralLendingDT(childId), // Implement if needed
                "DailyLiquidity" => await GetDailyLiquidityDT(childId),   // Implement if needed
                "InsiderLending" => await GetInsiderLendingDT(childId),   // Implement if needed*/

                // Other returns might be shared between DT and NWDT
                "OtherReturns" => isNWDT
                    ? await GetNWDTOtherReturnsDT(childId)
                    : await GetOtherReturnsDT(childId),

                _ => null // Unknown form type
            };
        }


        public async Task<ManagementReturnDTO?> GetNWDTManagementReturnDT(string childId)
        {
            var mgr = await _context.ManagementReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == childId);

            ManagementReturnDTO? managementReturn = null;

            if (mgr != null)
            {
                managementReturn = new ManagementReturnDTO
                {
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

            return managementReturn;
        }
        public async Task<NWDTInvestmentReturnDTO?> GetNWDTInvestmentReturnDT(string childId)
        {
            var inv = await _context.NWDTInvestmentReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == childId);

            NWDTInvestmentReturnDTO? nwdtInvestment = null;

            if (inv != null)
            {
                nwdtInvestment = new NWDTInvestmentReturnDTO
                {
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
        public async Task<List<OtherReturnDTO>> GetNWDTOtherReturnsDT(string childId)
        {
            var others = await _context.OtherReturns
                .AsNoTracking()
                .Where(o => o.Id == childId)
                .ToListAsync();

            var otherReturns = others.Select(o => new OtherReturnDTO
            {
                FormId = o.FormId,
                RequiresResubmission = o.RequiresResubmission,
                FormName = o.FormName,
                FileUrl = o.FileUrl
            }).ToList();

            return otherReturns;
        }
        public async Task<NWDTRiskClassificationDTO?> GetNWDTRiskClassificationDT(string childId)
        {
            var risks = await _context.NWDTRiskClassificationReturns
                .AsNoTracking()
                .Where(rc => rc.Id == childId)
                .ToListAsync();

            NWDTRiskClassificationDTO? nwdtRiskClassification = null;

            if (risks.Count > 0)
            {
                nwdtRiskClassification = new NWDTRiskClassificationDTO();

                var firstItem = risks.First();
                nwdtRiskClassification.FormId = firstItem.FormId;
                nwdtRiskClassification.RequiresResubmission = firstItem.RequiresResubmission;

                // Populate the NWDTRiskClassificationData list with all risk records
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
            }

            return nwdtRiskClassification;
        }

        public async Task<NWDTFinancialPositionDTO?> GetNWDTFinancialPositionDT(string childId)
        {
            var fp = await _context.NWDTFinancialPositionReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == childId);

            NWDTFinancialPositionDTO? nwdtFinancialPosition = null;

            if (fp != null)
            {
                nwdtFinancialPosition = new NWDTFinancialPositionDTO
                {
                    FormId = fp.FormId,
                    RequiresResubmission = fp.RequiresResubmission,
                    CashInHand = fp.CashInHand,
                    CashAtBank = fp.CashAtBank,
                    TotalCashAndCashEquivalent = fp.CashAndCashEquivalent,
                    PrepaymentsAndSundryReceivables = fp.PrepaymentsAndSundryReceivables,
                    GovernmentSecurities = fp.GovernmentSecurities,
                    OtherSecurities =
                        fp.PlacementInFinancialInstitutions +
                        fp.CommercialPapers +
                        fp.CollectiveInvestmentSchemes +
                        fp.Derivatives +
                        fp.EquityInvestments +
                        fp.InvestmentInCompanies,
                    BalancesWithOtherSaccos = 0M,
                    InvestmentsInCompanies = fp.InvestmentInCompanies,
                    TotalFinancialInvestments = fp.FinancialInvestments,
                    GrossLoanPortfolio = fp.GrossLoanPortfolio,
                    AllowanceForLoanLoss = fp.AllowanceForLoanLoss,
                    NetLoanPortfolio = fp.NetLoanPortfolio,
                    TaxRecoverable = fp.TaxRecoverable,
                    DeferredTaxAssets = fp.DeferredTaxAssets,
                    RetirementBenefitAssets = fp.RetirementBenefitAssets,
                    TotalAccountsReceivables = fp.AccountsReceivables,
                    InvestmentProperties = fp.InvestmentProperties,
                    PropertyAndEquipment = fp.PropertyAndEquipment,
                    PrepaidLeaseRentals = fp.PrepaidLeaseRentals,
                    IntangibleAssets = fp.IntangibleAssets,
                    OtherAssets = fp.OtherAssets,
                    TotalPropertyAndEquipment = fp.PropertyEquipmentOtherAssets,
                    TotalAssets = fp.TotalAssets,
                    SavingsDeposits = 0M,
                    ShortTermDeposits = 0M,
                    NonWithdrawableDeposits = fp.NonWithdrawableDeposits,
                    TotalDepositLiabilities = fp.TotalDepositLiabilities,
                    TaxPayable = fp.TaxPayable,
                    DividendsPayable = fp.DividendsPayable,
                    DeferredTaxLiability = fp.DeferredTaxLiability,
                    RetirementBenefitsLiability = fp.RetirementBenefitsLiability,
                    OtherLiabilities = fp.OtherLiabilities,
                    ExternalBorrowings = fp.ExternalBorrowings,
                    TotalAccountsPayable = fp.AccountsPayableOtherLiabilities,
                    TotalLiabilities = fp.TotalLiabilities,
                    ShareCapital = fp.ShareCapital,
                    CapitalGrants = fp.CapitalGrants,
                    PriorYearsRetainedEarnings = fp.PriorYearsRetainedEarnings,
                    CurrentYearSurplus = fp.CurrentYearSurplus,
                    TotalRetainedEarnings = fp.RetainedEarnings,
                    StatutoryReserve = fp.StatutoryReserve,
                    OtherReserves = fp.OtherReserves,
                    RevaluationReserves = fp.RevaluationReserves,
                    ProposedDividends = fp.ProposedDividends,
                    AdjustmentToEquity = fp.AdjustmentToEquity,
                    TotalOtherEquityAccounts = fp.OtherEquityAccounts,
                    TotalEquity = fp.TotalEquity,
                    //FilePath = fp.FilePath
                };
            }

            return nwdtFinancialPosition;
        }
        public async Task<NWDTLiquidityStatementDTO?> GetNWDTLiquidityStatementDT(string childId)
        {
            var liq = await _context.NDWTLiquidityReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == childId);

            NWDTLiquidityStatementDTO? nwdtLiquidityStatement = null;

            if (liq != null)
            {
                nwdtLiquidityStatement = new NWDTLiquidityStatementDTO
                {
                    FormId = liq.FormId,
                    RequiresResubmission = liq.RequiresResubmission,
                    LocalNotesAndCoins = liq.LocalNotesAndCoins,
                    ForeignNotesAndCoins = liq.ForeignNotesAndCoins,
                    BalancesWithCommercialBanks = liq.BalancesWithCommercialBanks,
                    TimeDepositsWithBanksMoreThan90Days = liq.TimeDepositsWithBanksMoreThan90Days,
                    OverdraftsAndMaturedLoans = liq.OverdraftsAndMaturedLoans,
                    BalancesWithOtherSaccoSocieties = liq.BalancesWithOtherSaccoSocieties,
                    BalancesWithOtherFinancialInstitutions = liq.BalancesWithOtherFinancialInstitutions,
                    BalancesDueToOtherSaccoSocieties = liq.BalancesDueToOtherSaccoSocieties,
                    BalancesDueToFinancialInstitutions = liq.BalancesDueToFinancialInstitutions,
                    TreasuryBills = liq.TreasuryBills,
                 /*   TreasuryBonds = liq.TreasuryBondsBearerBonds,
                    MaturedLiabilities = liq.MaturedLiabilities,
                    LiabilitiesMaturing91Days = liq.LiabilitiesMaturing91Days,
                    TotalNotesAndCoins = liq.TotalNotesAndCoins,
                    TotalGovernmentSecurities = liq.TotalGovernmentSecurities,
                    NetLiquidAssets = liq.NetLiquidAssets,
                    TotalOtherLiabilities = liq.TotalOtherLiabilities,
                    LiquidityRatio = liq.LiquidityRatio,
                    LiquidityRatioExcessDeficit = liq.LiquidityRatioExcessDeficit,
                    NetFinancialInstitutionBalances = liq.NetFinancialInstitutionBalances,
                    NetBankBalances = liq.NetBankBalances*/
                };
            }

            return nwdtLiquidityStatement;
        }
        public async Task<NWDTComprehesiveIncomeStatementDTO?> GetNWDTComprehensiveIncomeDT(string childId)
        {
            var inc = await _context.NWDTComprehensiveIncomeReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.Id == childId);

            NWDTComprehesiveIncomeStatementDTO? nwdtIncomeStatement = null;

            if (inc != null)
            {
                nwdtIncomeStatement = new NWDTComprehesiveIncomeStatementDTO
                {
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
                    //FilePath = inc.FilePath
                };
            }

            return nwdtIncomeStatement;
        }
        public async Task<NWDTDepositReturnDto?> GetNWDTDepositReturnDT(string childId)
        {
            var deposits = await _context.NWDTDepositReturns
                .AsNoTracking()
                .Where(dr => dr.Id == childId)
                .ToListAsync();

            NWDTDepositReturnDto? nwdtDepositReturn = null;

            if (deposits.Count > 0)
            {
                nwdtDepositReturn = new NWDTDepositReturnDto
                {
                    FormId = deposits[0].FormId ?? string.Empty,
                    RequiresResubmission = deposits[0].RequiresResubmission,
                    DepositReturnData = deposits.Select(dr => new NWDTDepositReturnData
                    {
                        RangeName = dr.RangeName ?? string.Empty,
                        DepositType = dr.DepositType ?? string.Empty,
                        NumberOfAccounts = dr.NumberOfAccounts,
                        Amount = dr.AmountInKshs000,
                        //FilePath = dr.FilePath ?? string.Empty
                    }).ToList()
                };
            }

            return nwdtDepositReturn;
        }
        public async Task<NWDTCapitalAdequacyDTO?> GetNWDTCapitalAdequacyDT(string childId)
        {
            var ca = await _context.NWDTCapitalAdequacyReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == childId);

            NWDTCapitalAdequacyDTO? nwdtCapitalAdequacy = null;

            if (ca != null)
            {
                nwdtCapitalAdequacy = new NWDTCapitalAdequacyDTO
                {
                    FormId = ca.Id,
                    RequiresResubmission = ca.RequiresResubmission,
                    ShareCapital = ca.ShareCapital,
                    StatutoryReserves = ca.StatutoryReserves,
                    /*RetainedEarningsAccumulatedLosses = ca.RetainedEarnings,
                    NetSurplusAfterTaxCurrentYearToDate = ca.NetSurplusAfterTax,*/
                    CapitalGrantsEquityInNature = ca.CapitalGrants,
                    OtherReserves = ca.OtherReserves,
                    SubTotalCoreCapital = ca.SubTotalCoreCapital,
/*                    InvestmentsInSubsidiaryAndEquityInstruments = ca.InvestmentsInSubsidiary,
*/                    OtherDeductions = ca.OtherDeductions,
                    TotalDeductions = ca.TotalDeductions,
                    CoreCapital = ca.CoreCapital,
                    //CashLocalAndForeignCurrency = ca.CashLocalForeign,
                    GovernmentSecurities = ca.GovernmentSecurities,
                    //DepositsAndBalancesAtOtherInstitutions = ca.DepositsBalancesAtOtherInstitutions,
                    LoansAndAdvances = ca.LoansAndAdvances,
                    Investments = ca.Investments,
                    PropertyAndEquipment = ca.PropertyAndEquipment,
                    OtherAssets = ca.OtherAssets,
                    TotalOnBalanceSheetAssets = ca.TotalOnBalanceSheetAssets,
                    TotalAssetsPerBalanceSheet = ca.TotalAssetsPerBalanceSheet,
                    //Difference = ca.DifferenceInAssets,
                    CoreCapitalToAssetsRatio = ca.CoreCapitalToAssetsRatio,
                    //CoreCapitalToAssetsRatioExcessDeficiency = ca.CoreCapitalToAssetsExcessDeficiency,
                    CoreCapitalToDepositsRatio = ca.CoreCapitalToDepositsRatio,
                    //CoreCapitalToDepositsRatioExcessDeficiency = ca.CoreCapitalToDepositsExcessDeficiency,
                    //FilePath = ca.FilePath
                };
            }

            return nwdtCapitalAdequacy;
        }

        public async Task<ManagementReturnDTO?> GetManagementReturnDT(string childId)
        {
            var managementEntity = await _context.ManagementReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == childId);

            ManagementReturnDTO? managementReturn = null;

            if (managementEntity != null)
            {
                managementReturn = new ManagementReturnDTO
                {
                    SaccoCsNumber = managementEntity.SaccoCsNumber,
                    GovernanceStructureScore = managementEntity.GorvenanceStructureScore,
                    GovernanceStructureWeight = managementEntity.GorvenanceStructureWeight,
                    GovernanceStructureWeightedScore = managementEntity.GorvenanceStructureWeightedScore,
                    InternalControlsScore = managementEntity.InternalControlsScore,
                    InternalControlsWeight = managementEntity.InternalControlsWeight,
                    InternalControlsWeightedScore = managementEntity.InternalControlsWeightedScore,
                    ComplianceWithLawsScore = managementEntity.ComplianceWithLawsAndRegulationsScore,
                    ComplianceWithLawsWeight = managementEntity.ComplianceWithLawsAndRegulationsWeight,
                    ComplianceWithLawsWeightedScore = managementEntity.ComplianceWithLawsAndRegulationsWeightedScore,
                    MemberProtectionScore = managementEntity.MemberProtectionScore,
                    MemberProtectionWeight = managementEntity.MemberProtectionWeight,
                    MemberProtectionWeightedScore = managementEntity.MemberProtectionWeightedScore,
                    AdequacyOfMISScore = managementEntity.AdequacyOfMISScore,
                    AdequacyOfMISWeight = managementEntity.AdequacyOfMISWeight,
                    AdequacyOfMISWeightedScore = managementEntity.AdequacyOfMISWeightedScore,
                    OverallRiskProfileScore = managementEntity.OverallRiskProfileScore,
                    OverallRiskProfileWeight = managementEntity.OverallRiskProfileWeight,
                    OverallRiskProfileWeightedScore = managementEntity.OverallRiskProfileWeightedScore,
                    MRating = managementEntity.MRating
                };
            }

            return managementReturn;
        }
        public async Task<InvestmentReturnDTO?> GetInvestmentReturnDT(string childId)
        {
            var investmentEntity = await _context.DTInvestmentReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(inv => inv.Id == childId);

            InvestmentReturnDTO? investment = null;

            if (investmentEntity != null)
            {
                investment = new InvestmentReturnDTO
                {
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
        public async Task<List<OtherReturnDTO>> GetOtherReturnsDT(string childId)
        {
            var otherReturnsEntities = await _context.OtherReturns
                .AsNoTracking()
                .Where(o => o.Id == childId)
                .ToListAsync();

            var otherReturns = new List<OtherReturnDTO>();

            foreach (var o in otherReturnsEntities)
            {
                otherReturns.Add(new OtherReturnDTO
                {
                    FormName = o.FormName,
                    FileUrl = o.FileUrl,
                    RequiresResubmission = o.RequiresResubmission,
                    FormId = o.FormId ?? string.Empty
                });
            }

            return otherReturns;
        }
        public async Task<RiskClassificationDTO?> GetRiskClassificationDT(string childId)
        {
            var riskEntities = await _context.DTRiskClassificationReturns
                .AsNoTracking()
                .Where(rc => rc.Id == childId)
                .ToListAsync();

            var riskClassifications = new RiskClassificationDTO();

            if (riskEntities.Any())
            {
                var firstEntity = riskEntities.First();
                riskClassifications.FormId = firstEntity.FormId ?? string.Empty;
                riskClassifications.RequiresResubmission = firstEntity.RequiresResubmission;

                foreach (var rc in riskEntities)
                {
                    riskClassifications.RiskClassificationData.Add(new RiskClassificationData
                    {
                        LoanType = rc.LoanType,
                        Classification = rc.Classification,
                        NumberOfAccounts = rc.NumberOfAccounts,
                        OutstandingLoanPortfolio = rc.OutstandingLoanPortfolio,
                        RequiredProvision = rc.RequiredProvision,
                        RequiredProvisionAmount = rc.RequiredProvisionAmount,
                        //FilePath = rc.FilePath
                    });
                }
            }

            return riskClassifications;
        }
        public async Task<LiquidityStatementDTO?> GetLiquidityStatementDT(string childId)
        {
            var liquidityEntity = await _context.DTLiquidityReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(liq => liq.Id == childId);

            LiquidityStatementDTO? liquidityStatement = null;

            if (liquidityEntity != null)
            {
                liquidityStatement = new LiquidityStatementDTO
                {
                    FormId = liquidityEntity.FormId ?? string.Empty,
                    RequiresResubmission = liquidityEntity.RequiresResubmission,
                    LocalNotesAndCoins = liquidityEntity.LocalNotesAndCoins,
                    ForeignNotesAndCoins = liquidityEntity.ForeignNotesAndCoins,
                    BalancesWithCommercialBanks = liquidityEntity.BalancesWithCommercialBanks,
                    TimeDepositsWithBanksMoreThan90Days = liquidityEntity.TimeDepositsWithBanksMoreThan90Days,
                    OverdraftsAndMaturedLoans = liquidityEntity.OverdraftsAndMaturedLoans,
                    BalancesWithOtherSaccoSocieties = liquidityEntity.BalancesWithOtherSaccoSocieties,
                    BalancesWithOtherFinancialInstitutions = liquidityEntity.BalancesWithOtherFinancialInstitutions,
                    BalancesDueToOtherSaccoSocieties = liquidityEntity.BalancesDueToOtherSaccoSocieties,
                    BalancesDueToFinancialInstitutions = liquidityEntity.BalancesDueToFinancialInstitutions,
                    TreasuryBills = liquidityEntity.TreasuryBills,
                    TreasuryBonds = liquidityEntity.TreasuryBonds,
                    DepositsFromMembers = liquidityEntity.DepositsFromMembers,
                    DepositsFromOtherSources = liquidityEntity.DepositsFromOtherSources,
                    MaturedLiabilities = liquidityEntity.MaturedLiabilities,
                    /*LiabilitiesMaturing91Days = liquidityEntity.LiabilitiesMaturing91Days,
                    TotalNotesAndCoins = liquidityEntity.TotalNotesAndCoins,
                    TotalGovernmentSecurities = liquidityEntity.TotalGovernmentSecurities,
                    NetLiquidAssets = liquidityEntity.NetLiquidAssets,
                    TotalDeposits = liquidityEntity.TotalDeposits,*/
                    TotalOtherLiabilities = liquidityEntity.TotalOtherLiabilities,
                    LiquidityRatio = liquidityEntity.LiquidityRatio,
                    LiquidityRatioExcessDeficit = liquidityEntity.LiquidityRatioExcessDeficit,
                    /*NetFinancialInstitutionBalances = liquidityEntity.NetFinancialInstitutionBalances,
                    NetBankBalances = liquidityEntity.NetBankBalances,*/
                    //FilePath = liquidityEntity.FilePath
                };
            }

            return liquidityStatement;
        }
        public async Task<FinancialPositionDTO?> GetFinancialPositionDT(string childId)
        {
            var balanceEntity = await _context.DTFinancialPositionReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(fp => fp.Id == childId);

            FinancialPositionDTO? financialPosition = null;

            if (balanceEntity != null)
            {
                financialPosition = new FinancialPositionDTO
                {
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
                    //FilePath = balanceEntity.FilePath
                };
            }

            return financialPosition;
        }
        public async Task<ComprehesiveIncomeStatementDTO?> GetComprehensiveIncomeDT(string childId)
        {
            var incomeEntity = await _context.DTComprehensiveIncomeReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.Id == childId);

            ComprehesiveIncomeStatementDTO? incomeStatement = null;

            if (incomeEntity != null)
            {
                incomeStatement = new ComprehesiveIncomeStatementDTO
                {
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
                    //FilePath = incomeEntity.FilePath
                };
            }

            return incomeStatement;
        }
        public async Task<CapitalAdequacyDTO?> GetCapitaAdequacyDT(string childId)
        {
            var capEntity = await _context.DTCapitalAdequacyReturns
                   .AsNoTracking()
                   .FirstOrDefaultAsync(ca => ca.Id == childId);

            CapitalAdequacyDTO? capitalAdequacy = null;
            int capitalDaysLate = 0;
            if (capEntity != null)
            {
                //capitalDaysLate = capEntity.DaysLateBy;
                capitalAdequacy = new CapitalAdequacyDTO
                {
                    FormId = capEntity.FormId ?? string.Empty,
                    RequiresResubmission = capEntity.RequiresResubmission,
                    ShareCapital = capEntity.ShareCapital,
                    StatutoryReserves = capEntity.StatutoryReserves,
                    RetainedEarningsAccumulatedLosses = capEntity.RetainedEarningsAccumulatedLosses,
                    NetSurplusAfterTaxCurrentYearToDate = capEntity.NetSurplusAfterTaxCurrentYearToDate,
                    CapitalGrantsEquityInNature = capEntity.CapitalGrantsEquityInNature,
                    GeneralReserves = capEntity.GeneralReserves,
                    OtherReserves = capEntity.OtherReserves,
                    SubTotalCoreCapital = capEntity.SubTotalCoreCapital,
                    InvestmentsInSubsidiaryAndEquityInstruments = capEntity.InvestmentsInSubsidiaryAndEquityInstruments,
                    OtherDeductions = capEntity.OtherDeductions,
                    TotalDeductions = capEntity.TotalDeductions,
                    CoreCapital = capEntity.CoreCapital,
                    InstitutionalCapital = capEntity.InstitutionalCapital,
                    CashLocalAndForeignCurrency = capEntity.CashLocalAndForeignCurrency,
                    GovernmentSecurities = capEntity.GovernmentSecurities,
                    DepositsAndBalancesAtOtherInstitutions = capEntity.DepositsAndBalancesAtOtherInstitutions,
                    LoansAndAdvances = capEntity.LoansAndAdvances,
                    Investments = capEntity.Investments,
                    PropertyAndEquipment = capEntity.PropertyAndEquipment,
                    OtherAssets = capEntity.OtherAssets,
                    TotalOnBalanceSheetAssets = capEntity.TotalOnBalanceSheetAssets,
                    TotalAssetsPerBalanceSheet = capEntity.TotalAssetsPerBalanceSheet,
                    Difference = capEntity.Difference,
                    CoreCapitalToAssetsRatio = capEntity.CoreCapitalToAssetsRatio,
                    CoreCapitalToAssetsRatioExcessDeficiency = capEntity.CoreCapitalToAssetsRatioExcessDeficiency,
                    InstitutionalCapitalToAssetsRatio = capEntity.InstitutionalCapitalToAssetsRatio,
                    InstitutionalCapitalToAssetsRatioExcessDeficiency = capEntity.InstitutionalCapitalToAssetsRatioExcessDeficiency,
                    CoreCapitalToDepositsRatio = capEntity.CoreCapitalToDepositsRatio,
                    CoreCapitalToDepositsRatioExcessDeficiency = capEntity.CoreCapitalToDepositsRatioExcessDeficiency,
                    //FilePath = capEntity.FilePath,
                };
            }

            return capitalAdequacy;

        }
        public async Task<DepositReturnDto?> GetDepositReturnDT(string childId)
        {
            var depositEntities = await _context.DepositReturns
                  .AsNoTracking()
                  .Where(dr => dr.Id == childId)
                  .ToListAsync();

            List<DepositReturnDto> depositReturnDtos = new List<DepositReturnDto>();
            DepositReturnDto? depositReturn = null;

            if (depositEntities == null || !depositEntities.Any())
            {
                return depositReturn;
            }

            depositReturn = new DepositReturnDto
            {
                FormId = depositEntities.FirstOrDefault()?.FormId ?? string.Empty,
                RequiresResubmission = depositEntities.FirstOrDefault()?.RequiresResubmission ?? false,
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
    }
}
