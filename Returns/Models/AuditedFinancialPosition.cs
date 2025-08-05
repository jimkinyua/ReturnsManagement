using System.ComponentModel.DataAnnotations.Schema;
using Returns.Models.Common;

namespace Returns.Models
{
    public class AuditedFinancialPosition : FormBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SaccoCsNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public decimal InterestRateOnDeposits { get; set; }
        public decimal RebateRateOnShareCapital { get; set; }
        public string ReturnSubmissionId { get; set; } = null!;
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
        // ASSETS
        public decimal CashAndCashEquivalent { get; set; }
        public decimal CashInHand { get; set; }  // Cash at Treasury and mobile wallets
        public decimal CashInMobileWallets { get; set; }  // MPESA, Airtel Money, Tangazo Letu etc.
        public decimal CashBalanceHeldInOtherSACCOs { get; set; }
        public decimal CashAtBankCurrentAccountsKsh { get; set; }
        public decimal CashAtBankFixedAccountsKsh { get; set; }
        public decimal CashAtBankDollarDenominated { get; set; }
        public decimal PrepaymentsAndSundryReceivables { get; set; }
        public decimal FinancialInvestments { get; set; }
        public decimal GovernmentSecuritiesTreasuryBillsBonds { get; set; }
        public decimal SavingsDepositsAtKUSCCO { get; set; }  // Jungu Kuu, etc.
        public decimal MoneyMarketAtCIC { get; set; }
        public decimal MoneyMarketAtCooperativeBank { get; set; }
        public decimal SavingsDepositsAtKenyaTeachersAssociationKETSA { get; set; }
        public decimal MoneyMarketOthers { get; set; }
        public decimal InvestmentSharesAtCooperativeBankAndCoopHoldings { get; set; }
        public decimal InvestmentSharesAtCIC { get; set; }
        public decimal InvestmentSharesAtKUSCCO { get; set; }
        public decimal InvestmentSharesInCooperativeAllianceOfKenyaCAK { get; set; }
        public decimal InvestmentSharesInCODIC { get; set; }
        public decimal InvestmentSharesInKenyaTeachersAssociationKETSA { get; set; }
        public decimal InvestmentInCompaniesAllSharesTradedAtNSE { get; set; }
        public decimal InvestmentInSACCOSubsidiaries { get; set; }
        public decimal InvestmentInKMRC { get; set; }
        public decimal InvestmentsInSaccoCentral { get; set; }
        public decimal AnyOtherInvestmentsNotListedAbove { get; set; }
        public decimal NetLoanPortfolio { get; set; }
        public decimal GrossLoanPortfolio { get; set; }
        public decimal AllowanceForLoanLoss { get; set; }
        public decimal AccountsReceivables { get; set; }
        public decimal TaxRecoverable { get; set; }
        public decimal DeferredTaxAssets { get; set; }
        public decimal RetirementBenefitAssets { get; set; }
        public decimal PropertyEquipmentAndOtherAssets { get; set; }
        public decimal InvestmentProperties { get; set; }
        public decimal PropertyAndEquipment { get; set; }
        public decimal PrepaidLeaseRentals { get; set; }
        public decimal IntangibleAssetsManagementInformationSystemCoreBanking { get; set; }
        public decimal IntangibleAssetsOthers { get; set; }
        public decimal OtherAssets { get; set; }
        public decimal TotalAssets { get; set; }

        // LIABILITIES
        public decimal SavingsDepositsWithdrawableDepositsFOSA { get; set; }
        public decimal ShortTermDeposits { get; set; }  // e.g., Fixed deposits, special savings
        public decimal NonWithdrawableDeposits { get; set; }  // BOSA member deposits
        public decimal TotalDepositLiabilities { get; set; }
        public decimal AccountsPayableAndOtherLiabilities { get; set; }
        public decimal TaxPayable { get; set; }
        public decimal DividendsPayable { get; set; }
        public decimal DeferredTaxLiability { get; set; }
        public decimal RetirementBenefitsLiability { get; set; }
        public decimal OtherLiabilities { get; set; }
        public decimal ExternalBorrowingsFromCommercialBanksAndMicroFinanceBanks { get; set; }
        public decimal ExternalBorrowingsFromKUSCCO { get; set; }
        public decimal ExternalBorrowingsFromWomenEnterpriseFunds { get; set; }
        public decimal ExternalBorrowingsFromMESPT { get; set; }
        public decimal ExternalBorrowingsFromAgricultureFinanceCorporationAFC { get; set; }
        public decimal ExternalBorrowingsFromYouthFund { get; set; }
        public decimal ExternalBorrowingsFromKMRC { get; set; }
        public decimal ExternalBorrowingsFromOtherInstitutions { get; set; }
        public decimal TotalLiabilities { get; set; }

        // EQUITY
        public decimal ShareCapital { get; set; }
        public decimal CapitalGrants { get; set; }
        public decimal RetainedEarnings { get; set; }
        public decimal PriorYearsRetainedEarnings { get; set; }
        public decimal CurrentYearsSurplus { get; set; }
        public decimal OtherEquityAccounts { get; set; }
        public decimal StatutoryReserve { get; set; }
        public decimal OtherReserves { get; set; }
        public decimal RevaluationReserves { get; set; }
        public decimal ProposedDividends { get; set; }
        public decimal AdjustmentToEquity { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal TotalLiabilitiesAndEquity { get; set; }
    }
}
