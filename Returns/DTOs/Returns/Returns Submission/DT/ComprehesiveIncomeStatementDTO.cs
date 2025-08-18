using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Submission.Returns_Submission.DT
{
    public class ComprehesiveIncomeStatementDTO : CommonFormDTO
    {
        // 1. Financial Income
        public decimal TotalFinancialIncome { get; set; }

        // 2. Financial Income from Loans Portfolio
        public decimal TotalFinancialIncomeFromLoans { get; set; }
        public decimal InterestOnLoanPortfolio { get; set; }
        public decimal FeesAndCommissionOnLoanPortfolio { get; set; }

        // 3. Financial Income from Investments
        public decimal TotalFinancialIncomeFromInvestments { get; set; }
        public decimal GovernmentSecurities { get; set; }
        public decimal DepositsWithBanks { get; set; }
        public decimal OtherInvestments { get; set; }
        public decimal OtherOperatingIncome { get; set; }

        // 4. Financial Expense
        public decimal TotalFinancialExpense { get; set; }
        public decimal InterestExpenseOnDeposits { get; set; }
        public decimal CostOfExternalBorrowings { get; set; }
        public decimal DividendExpenses { get; set; }
        public decimal OtherFinancialExpense { get; set; }
        public decimal FeesAndCommissionExpense { get; set; }
        public decimal OtherExpense { get; set; }

        // 5. Net Financial Income
        public decimal NetFinancialIncomeOrLoss { get; set; }

        // 6. Allowance for Loan Loss
        public decimal ProvisionForLoanLosses { get; set; }
        public decimal ValueOfLoansRecovered { get; set; }

        // 7. Operating Expenses
        public decimal TotalOperatingExpenses { get; set; }
        public decimal PersonnelExpenses { get; set; }
        public decimal GovernanceExpenses { get; set; }
        public decimal MarketingExpenses { get; set; }
        public decimal DepreciationAndAmortization { get; set; }
        public decimal AdministrativeExpenses { get; set; }

        // 8. Net Operating Income
        public decimal NetOperatingIncome { get; set; }

        // 9. Non-Operating Income/Expense
        public decimal NetNonOperatingIncome { get; set; }
        public decimal NonOperatingIncome { get; set; }
        public decimal NonOperatingExpense { get; set; }

        // 10. Net Income Before Taxes
        public decimal NetIncomeBeforeTaxesAndDonations { get; set; }

        // 11. Taxes
        public decimal TaxesPayable { get; set; }

        // 12. Net Income After Taxes
        public decimal NetIncomeAfterTaxes { get; set; }

        // 13 & 14. Donations and Final Net Income
        public decimal Donations { get; set; }
        public decimal NetIncomeAfterTaxesAndDonations { get; set; }

        public string FilePath { get; set; } = string.Empty;
    }
}
