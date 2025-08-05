using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AuditedComprehiveIncome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditedComprehensiveIncomes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReturnSubmissionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SaccoCsNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinancialIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialIncomeFromLoansPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestFromLoanPortfolioBosaLoans = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestFromLoanPortfolioFosaLoans = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestFromMobileLoans = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeesAndCommissionOnAllLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialIncomeFromInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecuritiesTreasuryBillsBonds = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SavingsDepositsAtKuscco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoneyMarketAtCic = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoneyMarketAtCooperativeBank = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SavingsDepositsAtKenyaTeachersAssociationKetsa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoneyMarketOthers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesAtCooperativeBankAndCoopHoldings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesAtCic = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesAtKuscco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesInCooperativeAllianceOfKenyaCak = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesInCodic = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentSharesInKenyaTeachersAssociationKetsa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentInCompaniesAllSharesTradedAtNse = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestFromFixedDepositsWithBanks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RentalIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherOperatingIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestPaidOnNonWithdrawableDepositsBosaDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestPaidOnFixedTermDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DividendExpensesOnMemberSharesCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestPaidOnExternalBorrowings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeesAndCommissionExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherFinancialExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetFinancialIncomeLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllowanceForLoanLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProvisionForLoanLosses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueOfLoansRecovered = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OperatingExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PersonnelSalariesAndWages = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PersonnelTrainingCosts = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherPersonnelExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernanceExpenseRelatedToBoardMembers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernanceExpenseRelatedToMembers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarketingExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepreciationAndAmortizationCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IctRelatedExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAdministrationExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetOperatingIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetNonOperatingIncomeExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonOperatingIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonOperatingExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetIncomeBeforeTaxesAndDonations = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Taxes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetIncomeAfterTaxesBeforeDonations = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Donations = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetIncomeAfterTaxesAndDonations = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    FormId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiresResubmission = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditedComprehensiveIncomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditedComprehensiveIncomes_ReturnSubmissions_ReturnSubmissionId",
                        column: x => x.ReturnSubmissionId,
                        principalTable: "ReturnSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditedComprehensiveIncomes_ReturnSubmissionId",
                table: "AuditedComprehensiveIncomes",
                column: "ReturnSubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditedComprehensiveIncomes");
        }
    }
}
