using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class DTFinancialPositionReturn : FormBase
    {
        // ASSETS
        // 1. Cash & Cash Equivalent
        public decimal CashInHand { get; set; }  // 1.1
        public decimal CashAtBank { get; set; }  // 1.2
        public decimal TotalCashAndCashEquivalent => CashInHand + CashAtBank;

        // 2. Prepayments & Sundry Receivables
        public decimal PrepaymentsAndSundryReceivables { get; set; }  // 2.0

        // 3. Financial Investments
        public decimal GovernmentSecurities { get; set; }  // 3.1
        public decimal OtherSecurities { get; set; }  // 3.2
        public decimal BalancesWithOtherSaccos { get; set; }  // 3.3a
        public decimal InvestmentsInCompanies { get; set; }  // 3.3b
        public decimal TotalFinancialInvestments => GovernmentSecurities + OtherSecurities +
                                                  BalancesWithOtherSaccos + InvestmentsInCompanies;

        // 4. Net Loan Portfolio
        public decimal GrossLoanPortfolio { get; set; }  // 4.1
        public decimal AllowanceForLoanLoss { get; set; }  // 4.2
        public decimal NetLoanPortfolio => GrossLoanPortfolio - AllowanceForLoanLoss;

        // 5. Accounts Receivables
        public decimal TaxRecoverable { get; set; }  // 5.1
        public decimal DeferredTaxAssets { get; set; }  // 5.2
        public decimal RetirementBenefitAssets { get; set; }  // 5.3
        public decimal TotalAccountsReceivables => TaxRecoverable + DeferredTaxAssets + RetirementBenefitAssets;

        // 6. Property & Equipment & Other assets
        public decimal InvestmentProperties { get; set; }  // 6.1
        public decimal PropertyAndEquipment { get; set; }  // 6.2
        public decimal PrepaidLeaseRentals { get; set; }  // 6.3
        public decimal IntangibleAssets { get; set; }  // 6.4
        public decimal OtherAssets { get; set; }  // 6.5
        public decimal TotalPropertyAndEquipment => InvestmentProperties + PropertyAndEquipment +
                                                  PrepaidLeaseRentals + IntangibleAssets + OtherAssets;

        public decimal TotalAssets => TotalCashAndCashEquivalent + PrepaymentsAndSundryReceivables +
                                     TotalFinancialInvestments + NetLoanPortfolio +
                                     TotalAccountsReceivables + TotalPropertyAndEquipment;

        // LIABILITIES
        public decimal SavingsDeposits { get; set; }  // 7
        public decimal ShortTermDeposits { get; set; }  // 8
        public decimal NonWithdrawableDeposits { get; set; }  // 9
        public decimal TotalDepositLiabilities => SavingsDeposits + ShortTermDeposits + NonWithdrawableDeposits;

        // 10. Accounts Payable & Other Liabilities
        public decimal TaxPayable { get; set; }  // 10.1
        public decimal DividendsPayable { get; set; }  // 10.2
        public decimal DeferredTaxLiability { get; set; }  // 10.3
        public decimal RetirementBenefitsLiability { get; set; }  // 10.4
        public decimal OtherLiabilities { get; set; }  // 10.5
        public decimal ExternalBorrowings { get; set; }  // 10.6
        public decimal TotalAccountsPayable => TaxPayable + DividendsPayable + DeferredTaxLiability +
                                             RetirementBenefitsLiability + OtherLiabilities + ExternalBorrowings;

        public decimal TotalLiabilities => TotalDepositLiabilities + TotalAccountsPayable;

        // EQUITY
        public decimal ShareCapital { get; set; }  // 11
        public decimal CapitalGrants { get; set; }  // 12

        // 13. Retained Earnings
        public decimal PriorYearsRetainedEarnings { get; set; }  // 13.1
        public decimal CurrentYearSurplus { get; set; }  // 13.2
        public decimal TotalRetainedEarnings => PriorYearsRetainedEarnings + CurrentYearSurplus;

        // 14. Other Equity Accounts
        public decimal StatutoryReserve { get; set; }
        public decimal OtherReserves { get; set; }
        public decimal RevaluationReserves { get; set; }
        public decimal ProposedDividends { get; set; }
        public decimal AdjustmentToEquity { get; set; }
        public decimal TotalOtherEquityAccounts => StatutoryReserve + OtherReserves +
                                                  RevaluationReserves + ProposedDividends + AdjustmentToEquity;

        public decimal TotalEquity => ShareCapital + CapitalGrants + TotalRetainedEarnings + TotalOtherEquityAccounts;

        public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;

        // related entities
        [ForeignKey("ReturnId")]
        public string ReturnId { get; set; } = null!;
        public virtual Return Return { get; set; } = null!;
        public string Year { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Frequency { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int DaysLateBy { get; set; }
        
        public Guid ReturnSubmissionId { get; set; }           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
    }
}
