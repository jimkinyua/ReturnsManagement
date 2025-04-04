using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class NWDTComprehensiveIncomeReturnTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NWDTComprehensiveIncomeReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InterestOnLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeesCommissionOnLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecuritiesIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlacementInBanksIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommercialPapersIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CollectiveInvestmentSchemesIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DerivativesIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EquityInvestmentsIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentInCompaniesIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestExpenseOnDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostOfExternalBorrowings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DividendExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherFinancialExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeesCommissionExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProvisionForLoanLosses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueOfLoansRecovered = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PersonnelExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernanceExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarketingExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepreciationAmortizationCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdministrativeExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonOperatingIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonOperatingExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Taxes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Donations = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialIncomeFromLoansPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialIncomeFromInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetFinancialIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllowanceForLoanLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OperatingExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetOperatingIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetNonOperatingIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetIncomeBeforeTaxes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetIncomeAfterTaxesBeforeDonations = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetIncomeAfterTaxesAndDonations = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NWDTComprehensiveIncomeReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NWDTComprehensiveIncomeReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NWDTComprehensiveIncomeReturns_ReturnId",
                table: "NWDTComprehensiveIncomeReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NWDTComprehensiveIncomeReturns");
        }
    }
}
