namespace Returns.DTOs.Returns_Submission.NWDT
{
    public class NWDTFinancialPositionDTO:CommonFormDTO
    {
        public decimal CashInHand { get; set; }
        public decimal CashAtBank { get; set; }
        public decimal TotalCashAndCashEquivalent;
        public decimal PrepaymentsAndSundryReceivables { get; set; }  // 2.0
        public decimal GovernmentSecurities { get; set; }  // 3.1
        public decimal OtherSecurities { get; set; }  // 3.2
        public decimal BalancesWithOtherSaccos { get; set; }  // 3.3a
        public decimal InvestmentsInCompanies { get; set; }  // 3.3b
        public decimal TotalFinancialInvestments;
        public decimal GrossLoanPortfolio { get; set; }  // 4.1
        public decimal AllowanceForLoanLoss { get; set; }  // 4.2
        public decimal NetLoanPortfolio;

        public decimal TaxRecoverable { get; set; }  // 5.1
        public decimal DeferredTaxAssets { get; set; }  // 5.2
        public decimal RetirementBenefitAssets { get; set; }  // 5.3
        public decimal TotalAccountsReceivables;
        public decimal InvestmentProperties { get; set; }  // 6.1
        public decimal PropertyAndEquipment { get; set; }  // 6.2
        public decimal PrepaidLeaseRentals { get; set; }  // 6.3
        public decimal IntangibleAssets { get; set; }  // 6.4
        public decimal OtherAssets { get; set; }  // 6.5
        public decimal TotalPropertyAndEquipment;
        public decimal TotalAssets;
        public decimal SavingsDeposits { get; set; }  // 7
        public decimal ShortTermDeposits { get; set; }  // 8
        public decimal NonWithdrawableDeposits { get; set; }  // 9
        public decimal TotalDepositLiabilities;
        public decimal TaxPayable { get; set; }  // 10.1
        public decimal DividendsPayable { get; set; }  // 10.2
        public decimal DeferredTaxLiability { get; set; }  // 10.3
        public decimal RetirementBenefitsLiability { get; set; }  // 10.4
        public decimal OtherLiabilities { get; set; }  // 10.5
        public decimal ExternalBorrowings { get; set; }  // 10.6
        public decimal TotalAccountsPayable;
        public decimal TotalLiabilities;
        public decimal ShareCapital { get; set; }  // 11
        public decimal CapitalGrants { get; set; }  // 12

        public decimal PriorYearsRetainedEarnings { get; set; }  // 13.1
        public decimal CurrentYearSurplus { get; set; }  // 13.2
        public decimal TotalRetainedEarnings;
        public decimal StatutoryReserve { get; set; }
        public decimal OtherReserves { get; set; }
        public decimal RevaluationReserves { get; set; }
        public decimal ProposedDividends { get; set; }
        public decimal AdjustmentToEquity { get; set; }
        public decimal TotalOtherEquityAccounts;
        public decimal TotalEquity;
        public string FilePath { get; set; } = string.Empty;
    }
}