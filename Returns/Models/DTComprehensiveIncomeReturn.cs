using Returns.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Returns.Models
{
    public class DTComprehensiveIncomeReturn : FormBase
    {
        // 1. Financial Income
        // 2. Financial Income from Loans Portfolio
        public decimal InterestOnLoanPortfolio { get; set; }  // 2.1
        public decimal FeesAndCommissionOnLoanPortfolio { get; set; }  // 2.2
        public decimal TotalFinancialIncomeFromLoans => InterestOnLoanPortfolio + FeesAndCommissionOnLoanPortfolio;

        // 3. Financial Income from Investments
        public decimal GovernmentSecurities { get; set; }  // 3.1
        public decimal DepositsWithBanks { get; set; }  // 3.2
        public decimal OtherInvestments { get; set; }  // 3.3
        public decimal OtherOperatingIncome { get; set; }  // 3.4
        public decimal TotalFinancialIncomeFromInvestments => GovernmentSecurities + DepositsWithBanks +
                                                             OtherInvestments + OtherOperatingIncome;

        public decimal TotalFinancialIncome => TotalFinancialIncomeFromLoans + TotalFinancialIncomeFromInvestments;

        // 4. Financial Expense
        public decimal InterestExpenseOnDeposits { get; set; }  // 4.2
        public decimal CostOfExternalBorrowings { get; set; }  // 4.3
        public decimal DividendExpenses { get; set; }  // 4.4
        public decimal OtherFinancialExpense { get; set; }  // 4.5
        public decimal FeesAndCommissionExpense { get; set; }  // 4.6
        public decimal OtherExpense { get; set; }  // 4.7
        public decimal TotalFinancialExpense => InterestExpenseOnDeposits + CostOfExternalBorrowings +
                                               DividendExpenses + OtherFinancialExpense +
                                               FeesAndCommissionExpense + OtherExpense;

        // 5. Net Financial Income
        public decimal NetFinancialIncome => TotalFinancialIncome - TotalFinancialExpense;

        // 6. Allowance for Loan Loss
        public decimal ProvisionForLoanLosses { get; set; }  // 6.1
        public decimal ValueOfLoansRecovered { get; set; }  // 6.2
        public decimal NetAllowanceForLoanLoss => ProvisionForLoanLosses - ValueOfLoansRecovered;

        // 7. Operating Expenses
        public decimal PersonnelExpenses { get; set; }  // 7.1
        public decimal GovernanceExpenses { get; set; }  // 7.2
        public decimal MarketingExpenses { get; set; }  // 7.3
        public decimal DepreciationAndAmortization { get; set; }  // 7.4
        public decimal AdministrativeExpenses { get; set; }  // 7.5
        public decimal TotalOperatingExpenses => PersonnelExpenses + GovernanceExpenses + MarketingExpenses +
                                               DepreciationAndAmortization + AdministrativeExpenses;

        // 8. Net Operating Income
        public decimal NetOperatingIncome => NetFinancialIncome - NetAllowanceForLoanLoss - TotalOperatingExpenses;

        // 9. Non-Operating Income/Expense
        public decimal NonOperatingIncome { get; set; }  // 9.1
        public decimal NonOperatingExpense { get; set; }  // 9.2
        public decimal NetNonOperatingIncome => NonOperatingIncome - NonOperatingExpense;

        // 10. Net Income Before Taxes
        public decimal NetIncomeBeforeTaxes => NetOperatingIncome + NetNonOperatingIncome;

        // 11. Taxes
        public decimal Taxes { get; set; }

        // 12. Net Income After Taxes
        public decimal NetIncomeAfterTaxes => NetIncomeBeforeTaxes - Taxes;

        // 13 & 14. Donations and Final Net Income
        public decimal Donations { get; set; }
        public decimal NetIncomeAfterTaxesAndDonations => NetIncomeAfterTaxes + Donations;

        [ForeignKey("ReturnId")]
        public string ReturnId { get; set; } = null!;
        public virtual Return Return { get; set; } = null!;
        public string Year { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Frequency { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int DaysLateBy
        {
            get; set;
        }
        
        public Guid ReturnSubmissionId { get; set; }           // FK → ReturnSubmission
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
    }
}
