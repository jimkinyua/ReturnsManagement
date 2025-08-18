using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class DTComprehensiveIncomeReturn : FormBase
    {
        // 1. Financial Income
        public decimal TotalFinancialIncome { get; set; }

        // 2. Financial Income from Loans Portfolio
        public decimal TotalFinancialIncomeFromLoans { get; set; }
        public decimal InterestOnLoanPortfolio { get; set; }  // 2.1
        public decimal FeesAndCommissionOnLoanPortfolio { get; set; }  // 2.2

        // 3. Financial Income from Investments
        public decimal TotalFinancialIncomeFromInvestments { get; set; }
        public decimal GovernmentSecurities { get; set; }  // 3.1
        public decimal DepositsWithBanks { get; set; }  // 3.2
        public decimal OtherInvestments { get; set; }  // 3.3
        public decimal OtherOperatingIncome { get; set; }  // 3.4

        // 4. Financial Expense
        public decimal TotalFinancialExpense { get; set; }
        public decimal InterestExpenseOnDeposits { get; set; }  // 4.2
        public decimal CostOfExternalBorrowings { get; set; }  // 4.3
        public decimal DividendExpenses { get; set; }  // 4.4
        public decimal OtherFinancialExpense { get; set; }  // 4.5
        public decimal FeesAndCommissionExpense { get; set; }  // 4.6
        public decimal OtherExpense { get; set; }  // 4.7

        // 5. Net Financial Income
        public decimal NetFinancialIncomeOrLoss { get; set; }

        // 6. Allowance for Loan Loss
        public decimal ProvisionForLoanLosses { get; set; }  // 6.1
        public decimal ValueOfLoansRecovered { get; set; }  // 6.2

        // 7. Operating Expenses
        public decimal TotalOperatingExpenses { get; set; }
        public decimal PersonnelExpenses { get; set; }  // 7.1
        public decimal GovernanceExpenses { get; set; }  // 7.2
        public decimal MarketingExpenses { get; set; }  // 7.3
        public decimal DepreciationAndAmortization { get; set; }  // 7.4
        public decimal AdministrativeExpenses { get; set; }  // 7.5

        // 8. Net Operating Income
        public decimal NetOperatingIncome { get; set; }

        // 9. Non-Operating Income/Expense
        public decimal NetNonOperatingIncome { get; set; }
        public decimal NonOperatingIncome { get; set; }  // 9.1
        public decimal NonOperatingExpense { get; set; }  // 9.2

        // 10. Net Income Before Taxes
        public decimal NetIncomeBeforeTaxesAndDonations { get; set; }

        // 11. Taxes
        public decimal TaxesPayable { get; set; }

        // 12. Net Income After Taxes
        public decimal NetIncomeAfterTaxes { get; set; }

        // 13 & 14. Donations and Final Net Income
        public decimal Donations { get; set; }
        public decimal NetIncomeAfterTaxesAndDonations { get; set; }

        public string Year { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [ForeignKey("ReturnSubmission")]
        public string ReturnSubmissionId { get; set; } = null!;
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
    }
}
