using Returns.Models.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class NWDTComprehensiveIncomeReturn : FormBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; } = null!;
        public string Frequency { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int DaysLateBy { get; set; }

        // Financial Income

        public decimal InterestOnLoanPortfolio { get; set; }

        
        public decimal FeesCommissionOnLoanPortfolio { get; set; }

        
        public decimal GovernmentSecuritiesIncome { get; set; }

        
        public decimal PlacementInBanksIncome { get; set; }

        
        public decimal CommercialPapersIncome { get; set; }

        
        public decimal CollectiveInvestmentSchemesIncome { get; set; }

        
        public decimal DerivativesIncome { get; set; }

        
        public decimal EquityInvestmentsIncome { get; set; }

        
        public decimal InvestmentInCompaniesIncome { get; set; }

        // Financial Expense
        
        public decimal InterestExpenseOnDeposits { get; set; }

        
        public decimal CostOfExternalBorrowings { get; set; }

        
        public decimal DividendExpenses { get; set; }

        
        public decimal OtherFinancialExpense { get; set; }

        
        public decimal FeesCommissionExpense { get; set; }
        
        public decimal OtherExpense { get; set; }
        // Loan Loss
        
        public decimal ProvisionForLoanLosses { get; set; }        
        public decimal ValueOfLoansRecovered { get; set; }

        // Operating Expenses
        
        public decimal PersonnelExpenses { get; set; }        
        public decimal GovernanceExpenses { get; set; }        
        public decimal MarketingExpenses { get; set; }        
        public decimal DepreciationAmortizationCharges { get; set; }        
        public decimal AdministrativeExpenses { get; set; }
        // Non-Operating Income/Expense
        
        public decimal NonOperatingIncome { get; set; }        
        public decimal NonOperatingExpense { get; set; }

        // Taxes and Donations
        
        public decimal Taxes { get; set; }
        
        public decimal Donations { get; set; }

        // Private backing fields for calculated values
        private decimal _financialIncome;
        private decimal _financialIncomeFromLoansPortfolio;
        private decimal _financialIncomeFromInvestments;
        private decimal _financialExpense;
        private decimal _netFinancialIncome;
        private decimal _allowanceForLoanLoss;
        private decimal _operatingExpenses;
        private decimal _netOperatingIncome;
        private decimal _netNonOperatingIncome;
        private decimal _netIncomeBeforeTaxes;
        private decimal _netIncomeAfterTaxesBeforeDonations;
        private decimal _netIncomeAfterTaxesAndDonations;

        // Calculated properties with backing fields for storage
        [NotMapped]
        public decimal FinancialIncomeFromLoansPortfolio
        {
            get => InterestOnLoanPortfolio + FeesCommissionOnLoanPortfolio;
            set => _financialIncomeFromLoansPortfolio = value;
        }

        [NotMapped]
        public decimal FinancialIncomeFromInvestments
        {
            get => GovernmentSecuritiesIncome + PlacementInBanksIncome +
                   CommercialPapersIncome + CollectiveInvestmentSchemesIncome +
                   DerivativesIncome + EquityInvestmentsIncome + InvestmentInCompaniesIncome;
            set => _financialIncomeFromInvestments = value;
        }

        [NotMapped]
        public decimal FinancialIncome
        {
            get => FinancialIncomeFromLoansPortfolio + FinancialIncomeFromInvestments;
            set => _financialIncome = value;
        }

        [NotMapped]
        public decimal FinancialExpense
        {
            get => InterestExpenseOnDeposits + CostOfExternalBorrowings +
                   DividendExpenses + OtherFinancialExpense +
                   FeesCommissionExpense + OtherExpense;
            set => _financialExpense = value;
        }

        [NotMapped]
        public decimal NetFinancialIncome
        {
            get => FinancialIncome - FinancialExpense;
            set => _netFinancialIncome = value;
        }

        [NotMapped]
        public decimal AllowanceForLoanLoss
        {
            get => ProvisionForLoanLosses - ValueOfLoansRecovered;
            set => _allowanceForLoanLoss = value;
        }

        [NotMapped]
        public decimal OperatingExpenses
        {
            get => PersonnelExpenses + GovernanceExpenses + MarketingExpenses +
                   DepreciationAmortizationCharges + AdministrativeExpenses;
            set => _operatingExpenses = value;
        }

        [NotMapped]
        public decimal NetOperatingIncome
        {
            get => NetFinancialIncome - AllowanceForLoanLoss - OperatingExpenses;
            set => _netOperatingIncome = value;
        }

        [NotMapped]
        public decimal NetNonOperatingIncome
        {
            get => NonOperatingIncome - NonOperatingExpense;
            set => _netNonOperatingIncome = value;
        }

        [NotMapped]
        public decimal NetIncomeBeforeTaxes
        {
            get => NetOperatingIncome + NetNonOperatingIncome;
            set => _netIncomeBeforeTaxes = value;
        }

        [NotMapped]
        public decimal NetIncomeAfterTaxesBeforeDonations
        {
            get => NetIncomeBeforeTaxes - Taxes;
            set => _netIncomeAfterTaxesBeforeDonations = value;
        }

        [NotMapped]
        public decimal NetIncomeAfterTaxesAndDonations
        {
            get => NetIncomeAfterTaxesBeforeDonations + Donations;
            set => _netIncomeAfterTaxesAndDonations = value;
        }

        // Database columns for calculated values
        [Column("FinancialIncomeFromLoansPortfolio", TypeName = "decimal(18,2)")]
        public decimal StoredFinancialIncomeFromLoansPortfolio
        {
            get => _financialIncomeFromLoansPortfolio == 0 ? FinancialIncomeFromLoansPortfolio : _financialIncomeFromLoansPortfolio;
            set => _financialIncomeFromLoansPortfolio = value;
        }

        [Column("FinancialIncomeFromInvestments", TypeName = "decimal(18,2)")]
        public decimal StoredFinancialIncomeFromInvestments
        {
            get => _financialIncomeFromInvestments == 0 ? FinancialIncomeFromInvestments : _financialIncomeFromInvestments;
            set => _financialIncomeFromInvestments = value;
        }

        [Column("FinancialIncome", TypeName = "decimal(18,2)")]
        public decimal StoredFinancialIncome
        {
            get => _financialIncome == 0 ? FinancialIncome : _financialIncome;
            set => _financialIncome = value;
        }

        [Column("FinancialExpense", TypeName = "decimal(18,2)")]
        public decimal StoredFinancialExpense
        {
            get => _financialExpense == 0 ? FinancialExpense : _financialExpense;
            set => _financialExpense = value;
        }

        [Column("NetFinancialIncome", TypeName = "decimal(18,2)")]
        public decimal StoredNetFinancialIncome
        {
            get => _netFinancialIncome == 0 ? NetFinancialIncome : _netFinancialIncome;
            set => _netFinancialIncome = value;
        }

        [Column("AllowanceForLoanLoss", TypeName = "decimal(18,2)")]
        public decimal StoredAllowanceForLoanLoss
        {
            get => _allowanceForLoanLoss == 0 ? AllowanceForLoanLoss : _allowanceForLoanLoss;
            set => _allowanceForLoanLoss = value;
        }

        [Column("OperatingExpenses", TypeName = "decimal(18,2)")]
        public decimal StoredOperatingExpenses
        {
            get => _operatingExpenses == 0 ? OperatingExpenses : _operatingExpenses;
            set => _operatingExpenses = value;
        }

        [Column("NetOperatingIncome", TypeName = "decimal(18,2)")]
        public decimal StoredNetOperatingIncome
        {
            get => _netOperatingIncome == 0 ? NetOperatingIncome : _netOperatingIncome;
            set => _netOperatingIncome = value;
        }

        [Column("NetNonOperatingIncome", TypeName = "decimal(18,2)")]
        public decimal StoredNetNonOperatingIncome
        {
            get => _netNonOperatingIncome == 0 ? NetNonOperatingIncome : _netNonOperatingIncome;
            set => _netNonOperatingIncome = value;
        }

        [Column("NetIncomeBeforeTaxes", TypeName = "decimal(18,2)")]
        public decimal StoredNetIncomeBeforeTaxes
        {
            get => _netIncomeBeforeTaxes == 0 ? NetIncomeBeforeTaxes : _netIncomeBeforeTaxes;
            set => _netIncomeBeforeTaxes = value;
        }

        [Column("NetIncomeAfterTaxesBeforeDonations", TypeName = "decimal(18,2)")]
        public decimal StoredNetIncomeAfterTaxesBeforeDonations
        {
            get => _netIncomeAfterTaxesBeforeDonations == 0 ? NetIncomeAfterTaxesBeforeDonations : _netIncomeAfterTaxesBeforeDonations;
            set => _netIncomeAfterTaxesBeforeDonations = value;
        }

        [Column("NetIncomeAfterTaxesAndDonations", TypeName = "decimal(18,2)")]
        public decimal StoredNetIncomeAfterTaxesAndDonations
        {
            get => _netIncomeAfterTaxesAndDonations == 0 ? NetIncomeAfterTaxesAndDonations : _netIncomeAfterTaxesAndDonations;
            set => _netIncomeAfterTaxesAndDonations = value;
        }

        // Method to calculate and store all totals
        public void CalculateAndStoreTotals()
        {
            StoredFinancialIncomeFromLoansPortfolio = FinancialIncomeFromLoansPortfolio;
            StoredFinancialIncomeFromInvestments = FinancialIncomeFromInvestments;
            StoredFinancialIncome = FinancialIncome;
            StoredFinancialExpense = FinancialExpense;
            StoredNetFinancialIncome = NetFinancialIncome;
            StoredAllowanceForLoanLoss = AllowanceForLoanLoss;
            StoredOperatingExpenses = OperatingExpenses;
            StoredNetOperatingIncome = NetOperatingIncome;
            StoredNetNonOperatingIncome = NetNonOperatingIncome;
            StoredNetIncomeBeforeTaxes = NetIncomeBeforeTaxes;
            StoredNetIncomeAfterTaxesBeforeDonations = NetIncomeAfterTaxesBeforeDonations;
            StoredNetIncomeAfterTaxesAndDonations = NetIncomeAfterTaxesAndDonations;
        }

        // Foreign key relationship
        [ForeignKey("ReturnId")]
        public string ReturnId { get; set; } = null!;
        public virtual Return Return { get; set; } = null!;
        
        public Guid ReturnSubmissionId { get; set; }           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
    }
}