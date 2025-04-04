using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddNWDTFinancialPositionReturnTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NWDTFinancialPositionReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashInHand = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashAtBank = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashAndCashEquivalent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrepaymentsAndSundryReceivables = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlacementInFinancialInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommercialPapers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CollectiveInvestmentSchemes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Derivatives = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EquityInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentInCompanies = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllowanceForLoanLoss = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRecoverable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeferredTaxAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetirementBenefitAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountsReceivables = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentProperties = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyAndEquipment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrepaidLeaseRentals = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IntangibleAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyEquipmentOtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonWithdrawableDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DividendsPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeferredTaxLiability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetirementBenefitsLiability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalBorrowings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountsPayableOtherLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CapitalGrants = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriorYearsRetainedEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentYearSurplus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatutoryReserve = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RevaluationReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProposedDividends = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdjustmentToEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherEquityAccounts = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalLiabilitiesAndEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NWDTFinancialPositionReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NWDTFinancialPositionReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NWDTFinancialPositionReturns_ReturnId",
                table: "NWDTFinancialPositionReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NWDTFinancialPositionReturns");
        }
    }
}
