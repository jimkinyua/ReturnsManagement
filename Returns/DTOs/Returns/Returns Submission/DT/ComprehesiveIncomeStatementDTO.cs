using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Returns.DTOs.Returns_Submission.Returns_Submission.DT
{
    public class ComprehesiveIncomeStatementDTO
    {
        public decimal InterestOnLoanPortfolio { get; set; }
        public decimal FeesAndCommissionOnLoanPortfolio { get; set; }
        public decimal TotalFinancialIncomeFromLoans;
        public decimal GovernmentSecurities { get; set; }
        public decimal DepositsWithBanks { get; set; }
        public decimal OtherInvestments { get; set; }
        public decimal OtherOperatingIncome { get; set; }
        public decimal TotalFinancialIncomeFromInvestments;
        public decimal TotalFinancialIncome;

        public decimal InterestExpenseOnDeposits { get; set; }
        public decimal CostOfExternalBorrowings { get; set; }
        public decimal DividendExpenses { get; set; }
        public decimal OtherFinancialExpense { get; set; }
        public decimal FeesAndCommissionExpense { get; set; }
        public decimal OtherExpense { get; set; }
        public decimal TotalFinancialExpense;
        public decimal NetFinancialIncome;

        public decimal ProvisionForLoanLosses { get; set; }  // 6.1
        public decimal ValueOfLoansRecovered { get; set; }  // 6.2
        public decimal NetAllowanceForLoanLoss;

        public decimal PersonnelExpenses { get; set; }  // 7.1
        public decimal GovernanceExpenses { get; set; }  // 7.2
        public decimal MarketingExpenses { get; set; }  // 7.3
        public decimal DepreciationAndAmortization { get; set; }  // 7.4
        public decimal AdministrativeExpenses { get; set; }  // 7.5
        public decimal TotalOperatingExpenses;
        public decimal NetOperatingIncome;
        public decimal NonOperatingIncome { get; set; }  // 9.1
        public decimal NonOperatingExpense { get; set; }  // 9.2
        public decimal NetNonOperatingIncome;
        public decimal NetIncomeBeforeTaxes;
        public decimal Taxes { get; set; }
        public decimal NetIncomeAfterTaxes;
        public decimal Donations { get; set; }
        public decimal NetIncomeAfterTaxesAndDonations;
        public string FilePath { get; set; } = string.Empty;

    }
}
