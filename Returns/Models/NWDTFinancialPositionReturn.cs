using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class NWDTFinancialPositionReturn : FormBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal CashInHand { get; set; }
        public decimal CashAtBank { get; set; }

        // Calculated Cash and Cash Equivalent with storage field
        private decimal _cashAndCashEquivalent;

        [NotMapped]
        public decimal CashAndCashEquivalent
        {
            get => CashInHand + CashAtBank;
            set => _cashAndCashEquivalent = value;
        }

        [Column("CashAndCashEquivalent", TypeName = "decimal(18,2)")]
        public decimal StoredCashAndCashEquivalent
        {
            get => _cashAndCashEquivalent == 0 ? CashAndCashEquivalent : _cashAndCashEquivalent;
            set => _cashAndCashEquivalent = value;
        }

        // Prepayments & Sundry Receivables

        public decimal PrepaymentsAndSundryReceivables { get; set; }
        // Financial Investments Section
        public decimal GovernmentSecurities { get; set; }
        public decimal PlacementInFinancialInstitutions { get; set; }
        public decimal CommercialPapers { get; set; }
        public decimal CollectiveInvestmentSchemes { get; set; }
        public decimal Derivatives { get; set; }
        public decimal EquityInvestments { get; set; }
        public decimal InvestmentInCompanies { get; set; }

        // Calculated Financial Investments with storage field
        private decimal _financialInvestments;

        [NotMapped]
        public decimal FinancialInvestments
        {
            get => GovernmentSecurities + PlacementInFinancialInstitutions +
                   CommercialPapers + CollectiveInvestmentSchemes +
                   Derivatives + EquityInvestments + InvestmentInCompanies;
            set => _financialInvestments = value;
        }

        [Column("FinancialInvestments", TypeName = "decimal(18,2)")]
        public decimal StoredFinancialInvestments
        {
            get => _financialInvestments == 0 ? FinancialInvestments : _financialInvestments;
            set => _financialInvestments = value;
        }

        // Loan Portfolio Section

        public decimal GrossLoanPortfolio { get; set; }
        public decimal AllowanceForLoanLoss { get; set; }

        // Calculated Net Loan Portfolio with storage field
        private decimal _netLoanPortfolio;

        [NotMapped]
        public decimal NetLoanPortfolio
        {
            get => GrossLoanPortfolio - AllowanceForLoanLoss;
            set => _netLoanPortfolio = value;
        }

        [Column("NetLoanPortfolio", TypeName = "decimal(18,2)")]
        public decimal StoredNetLoanPortfolio
        {
            get => _netLoanPortfolio == 0 ? NetLoanPortfolio : _netLoanPortfolio;
            set => _netLoanPortfolio = value;
        }

        // Accounts Receivables Section

        public decimal TaxRecoverable { get; set; }
        public decimal DeferredTaxAssets { get; set; }
        public decimal RetirementBenefitAssets { get; set; }

        // Calculated Accounts Receivables with storage field
        private decimal _accountsReceivables;

        [NotMapped]
        public decimal AccountsReceivables
        {
            get => TaxRecoverable + DeferredTaxAssets + RetirementBenefitAssets;
            set => _accountsReceivables = value;
        }

        [Column("AccountsReceivables", TypeName = "decimal(18,2)")]
        public decimal StoredAccountsReceivables
        {
            get => _accountsReceivables == 0 ? AccountsReceivables : _accountsReceivables;
            set => _accountsReceivables = value;
        }

        // Property & Equipment Section
        public decimal InvestmentProperties { get; set; }
        public decimal PropertyAndEquipment { get; set; }
        public decimal PrepaidLeaseRentals { get; set; }
        public decimal IntangibleAssets { get; set; }
        public decimal OtherAssets { get; set; }

        // Calculated Property Equipment and Other Assets with storage field
        private decimal _propertyEquipmentOtherAssets;

        [NotMapped]
        public decimal PropertyEquipmentOtherAssets
        {
            get => InvestmentProperties + PropertyAndEquipment +
                   PrepaidLeaseRentals + IntangibleAssets + OtherAssets;
            set => _propertyEquipmentOtherAssets = value;
        }

        [Column("PropertyEquipmentOtherAssets", TypeName = "decimal(18,2)")]
        public decimal StoredPropertyEquipmentOtherAssets
        {
            get => _propertyEquipmentOtherAssets == 0 ? PropertyEquipmentOtherAssets : _propertyEquipmentOtherAssets;
            set => _propertyEquipmentOtherAssets = value;
        }

        // Total Assets with storage field
        private decimal _totalAssets;

        [NotMapped]
        public decimal TotalAssets
        {
            get => CashAndCashEquivalent + PrepaymentsAndSundryReceivables +
                   FinancialInvestments + NetLoanPortfolio +
                   AccountsReceivables + PropertyEquipmentOtherAssets;
            set => _totalAssets = value;
        }

        [Column("TotalAssets", TypeName = "decimal(18,2)")]
        public decimal StoredTotalAssets
        {
            get => _totalAssets == 0 ? TotalAssets : _totalAssets;
            set => _totalAssets = value;
        }

        // LIABILITIES
        // Deposits Section

        public decimal NonWithdrawableDeposits { get; set; }

        // Total Deposit Liabilities with storage field
        private decimal _totalDepositLiabilities;

        [NotMapped]
        public decimal TotalDepositLiabilities
        {
            get => NonWithdrawableDeposits;
            set => _totalDepositLiabilities = value;
        }

        [Column("TotalDepositLiabilities", TypeName = "decimal(18,2)")]
        public decimal StoredTotalDepositLiabilities
        {
            get => _totalDepositLiabilities == 0 ? TotalDepositLiabilities : _totalDepositLiabilities;
            set => _totalDepositLiabilities = value;
        }

        // Accounts Payable Section

        public decimal TaxPayable { get; set; }
        public decimal DividendsPayable { get; set; }
        public decimal DeferredTaxLiability { get; set; }
        public decimal RetirementBenefitsLiability { get; set; }
        public decimal OtherLiabilities { get; set; }
        public decimal ExternalBorrowings { get; set; }

        // Calculated Accounts Payable and Other Liabilities with storage field
        private decimal _accountsPayableOtherLiabilities;

        [NotMapped]
        public decimal AccountsPayableOtherLiabilities
        {
            get => TaxPayable + DividendsPayable + DeferredTaxLiability +
                   RetirementBenefitsLiability + OtherLiabilities + ExternalBorrowings;
            set => _accountsPayableOtherLiabilities = value;
        }

        [Column("AccountsPayableOtherLiabilities", TypeName = "decimal(18,2)")]
        public decimal StoredAccountsPayableOtherLiabilities
        {
            get => _accountsPayableOtherLiabilities == 0 ? AccountsPayableOtherLiabilities : _accountsPayableOtherLiabilities;
            set => _accountsPayableOtherLiabilities = value;
        }

        // Total Liabilities with storage field
        private decimal _totalLiabilities;

        [NotMapped]
        public decimal TotalLiabilities
        {
            get => TotalDepositLiabilities + AccountsPayableOtherLiabilities;
            set => _totalLiabilities = value;
        }

        [Column("TotalLiabilities", TypeName = "decimal(18,2)")]
        public decimal StoredTotalLiabilities
        {
            get => _totalLiabilities == 0 ? TotalLiabilities : _totalLiabilities;
            set => _totalLiabilities = value;
        }

        // EQUITY
        // Share Capital        
        public decimal ShareCapital { get; set; }

        // Capital Grants        
        public decimal CapitalGrants { get; set; }

        // Retained Earnings Section
        public decimal PriorYearsRetainedEarnings { get; set; }
        public decimal CurrentYearSurplus { get; set; }

        private decimal _retainedEarnings;

        [NotMapped]
        public decimal RetainedEarnings
        {
            get => PriorYearsRetainedEarnings + CurrentYearSurplus;
            set => _retainedEarnings = value;
        }

        [Column("RetainedEarnings", TypeName = "decimal(18,2)")]
        public decimal StoredRetainedEarnings
        {
            get => _retainedEarnings == 0 ? RetainedEarnings : _retainedEarnings;
            set => _retainedEarnings = value;
        }

        // Other Equity Accounts Section
        public decimal StatutoryReserve { get; set; }
        public decimal OtherReserves { get; set; }
        public decimal RevaluationReserves { get; set; }
        public decimal ProposedDividends { get; set; }
        public decimal AdjustmentToEquity { get; set; }

        // Calculated Other Equity Accounts with storage field
        private decimal _otherEquityAccounts;

        [NotMapped]
        public decimal OtherEquityAccounts
        {
            get => StatutoryReserve + OtherReserves + RevaluationReserves +
                   ProposedDividends + AdjustmentToEquity;
            set => _otherEquityAccounts = value;
        }

        [Column("OtherEquityAccounts", TypeName = "decimal(18,2)")]
        public decimal StoredOtherEquityAccounts
        {
            get => _otherEquityAccounts == 0 ? OtherEquityAccounts : _otherEquityAccounts;
            set => _otherEquityAccounts = value;
        }

        // Total Equity with storage field
        private decimal _totalEquity;

        [NotMapped]
        public decimal TotalEquity
        {
            get => ShareCapital + CapitalGrants + RetainedEarnings + OtherEquityAccounts;
            set => _totalEquity = value;
        }

        [Column("TotalEquity", TypeName = "decimal(18,2)")]
        public decimal StoredTotalEquity
        {
            get => _totalEquity == 0 ? TotalEquity : _totalEquity;
            set => _totalEquity = value;
        }

        // Total Liabilities and Equity with storage field
        private decimal _totalLiabilitiesAndEquity;

        [NotMapped]
        public decimal TotalLiabilitiesAndEquity
        {
            get => TotalLiabilities + TotalEquity;
            set => _totalLiabilitiesAndEquity = value;
        }

        [Column("TotalLiabilitiesAndEquity", TypeName = "decimal(18,2)")]
        public decimal StoredTotalLiabilitiesAndEquity
        {
            get => _totalLiabilitiesAndEquity == 0 ? TotalLiabilitiesAndEquity : _totalLiabilitiesAndEquity;
            set => _totalLiabilitiesAndEquity = value;
        }

        // Method to calculate and store all totals
        public void CalculateAndStoreTotals()
        {
            StoredCashAndCashEquivalent = CashAndCashEquivalent;
            StoredFinancialInvestments = FinancialInvestments;
            StoredNetLoanPortfolio = NetLoanPortfolio;
            StoredAccountsReceivables = AccountsReceivables;
            StoredPropertyEquipmentOtherAssets = PropertyEquipmentOtherAssets;
            StoredTotalAssets = TotalAssets;
            StoredTotalDepositLiabilities = TotalDepositLiabilities;
            StoredAccountsPayableOtherLiabilities = AccountsPayableOtherLiabilities;
            StoredTotalLiabilities = TotalLiabilities;
            StoredRetainedEarnings = RetainedEarnings;
            StoredOtherEquityAccounts = OtherEquityAccounts;
            StoredTotalEquity = TotalEquity;
            StoredTotalLiabilitiesAndEquity = TotalLiabilitiesAndEquity;
        }
     /*   public string Period { get; set; } = null!;
        public string Frequency { get; set; } = null!;
        public string FilePath { get; set; } = null!;*/
        public int DaysLateBy { get; set; }
        [ForeignKey("ReturnSubmission")]
        public string ReturnSubmissionId { get; set; } = null!;           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
    }
}
