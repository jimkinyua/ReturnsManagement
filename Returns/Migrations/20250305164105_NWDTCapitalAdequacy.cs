using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class NWDTCapitalAdequacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOtherForm",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "NDWTCapitalAdequacyReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatutoryReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarningsAccumulatedLosses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSurplusAfterTaxCurrentYearToDate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CapitalGrantsEquityInNature = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GeneralReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotalCoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvestmentsInSubsidiaryAndEquityInstruments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetainedEarningsAndDisclosedReserves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashLocalAndForeignCurrency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GovernmentSecurities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositsAndBalancesAtOtherInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoansAndAdvances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Investments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyAndEquipment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOnBalanceSheetAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssetsPerBalanceSheet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Difference = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOffBalanceSheetAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssetsOnBalanceSheet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AssetValueOffBalanceSheet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDepositsLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumCoreCapitalToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumRetainedEarningsAndDisclosedReservesToAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumCoreCapitalToDepositsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NDWTCapitalAdequacyReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NDWTCapitalAdequacyReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NDWTCapitalAdequacyReturns_ReturnId",
                table: "NDWTCapitalAdequacyReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NDWTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "IsOtherForm",
                table: "ReturnForms");
        }
    }
}
