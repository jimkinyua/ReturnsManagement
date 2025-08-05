using ClosedXML.Excel;
using Returns.Helpers.Interfaces;
using Returns.Models;
using Returns.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Returns.Models
{
    public class AuditedComprehensiveIncome:FormBase
    {
        [ForeignKey("ReturnSubmission")]
        public string ReturnSubmissionId { get; set; } = string.Empty;
        public ReturnSubmission ReturnSubmission { get; set; } = null!;
        public string SaccoCsNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Financial Income
        public decimal FinancialIncome { get; set; }
        public decimal FinancialIncomeFromLoansPortfolio { get; set; }
        public decimal InterestFromLoanPortfolioBosaLoans { get; set; }
        public decimal InterestFromLoanPortfolioFosaLoans { get; set; }
        public decimal InterestFromMobileLoans { get; set; }
        public decimal FeesAndCommissionOnAllLoanPortfolio { get; set; }

        // Financial Income from Investments
        public decimal FinancialIncomeFromInvestments { get; set; }
        public decimal GovernmentSecuritiesTreasuryBillsBonds { get; set; }
        public decimal SavingsDepositsAtKuscco { get; set; }
        public decimal MoneyMarketAtCic { get; set; }
        public decimal MoneyMarketAtCooperativeBank { get; set; }
        public decimal SavingsDepositsAtKenyaTeachersAssociationKetsa { get; set; }
        public decimal MoneyMarketOthers { get; set; }
        public decimal InvestmentSharesAtCooperativeBankAndCoopHoldings { get; set; }
        public decimal InvestmentSharesAtCic { get; set; }
        public decimal InvestmentSharesAtKuscco { get; set; }
        public decimal InvestmentSharesInCooperativeAllianceOfKenyaCak { get; set; }
        public decimal InvestmentSharesInCodic { get; set; }
        public decimal InvestmentSharesInKenyaTeachersAssociationKetsa { get; set; }
        public decimal InvestmentInCompaniesAllSharesTradedAtNse { get; set; }
        public decimal InterestFromFixedDepositsWithBanks { get; set; }
        public decimal RentalIncome { get; set; }
        public decimal OtherOperatingIncome { get; set; }

        // Financial Expense
        public decimal FinancialExpense { get; set; }
        public decimal InterestPaidOnNonWithdrawableDepositsBosaDeposits { get; set; }
        public decimal InterestPaidOnFixedTermDeposits { get; set; }
        public decimal DividendExpensesOnMemberSharesCapital { get; set; }
        public decimal InterestPaidOnExternalBorrowings { get; set; }
        public decimal FeesAndCommissionExpense { get; set; }
        public decimal OtherFinancialExpense { get; set; }
        public decimal NetFinancialIncomeLoss { get; set; }

        // Allowance for Loan Loss
        public decimal AllowanceForLoanLoss { get; set; }
        public decimal ProvisionForLoanLosses { get; set; }
        public decimal ValueOfLoansRecovered { get; set; }

        // Operating Expenses
        public decimal OperatingExpenses { get; set; }
        public decimal PersonnelSalariesAndWages { get; set; }
        public decimal PersonnelTrainingCosts { get; set; }
        public decimal OtherPersonnelExpense { get; set; }
        public decimal GovernanceExpenseRelatedToBoardMembers { get; set; }
        public decimal GovernanceExpenseRelatedToMembers { get; set; }
        public decimal MarketingExpenses { get; set; }
        public decimal DepreciationAndAmortizationCharges { get; set; }
        public decimal IctRelatedExpense { get; set; }
        public decimal OtherAdministrationExpenses { get; set; }
        public decimal NetOperatingIncome { get; set; }

        // Net Non-Operating Income/Expense
        public decimal NetNonOperatingIncomeExpense { get; set; }
        public decimal NonOperatingIncome { get; set; }
        public decimal NonOperatingExpense { get; set; }
        public decimal NetIncomeBeforeTaxesAndDonations { get; set; }
        public decimal Taxes { get; set; }
        public decimal NetIncomeAfterTaxesBeforeDonations { get; set; }
        public decimal Donations { get; set; }
        public decimal NetIncomeAfterTaxesAndDonations { get; set; }
    }
}
