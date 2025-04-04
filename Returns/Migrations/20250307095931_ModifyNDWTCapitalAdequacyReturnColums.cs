using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class ModifyNDWTCapitalAdequacyReturnColums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalOffBalanceSheetAssets",
                table: "NDWTCapitalAdequacyReturns",
                newName: "TotalAssets");

            migrationBuilder.RenameColumn(
                name: "TotalAssetsOnBalanceSheet",
                table: "NDWTCapitalAdequacyReturns",
                newName: "RetainedEarningsToCoreCaptialRatio");

            migrationBuilder.RenameColumn(
                name: "RetainedEarningsAccumulatedLosses",
                table: "NDWTCapitalAdequacyReturns",
                newName: "RetainedEarningsToCoreCaptialExcessDeficiency");

            migrationBuilder.RenameColumn(
                name: "NetSurplusAfterTaxCurrentYearToDate",
                table: "NDWTCapitalAdequacyReturns",
                newName: "RetainedEarnings");

            migrationBuilder.RenameColumn(
                name: "MinimumRetainedEarningsAndDisclosedReservesToAssetsRatio",
                table: "NDWTCapitalAdequacyReturns",
                newName: "OffBalanceSheetAssets");

            migrationBuilder.RenameColumn(
                name: "MinimumCoreCapitalToDepositsRatio",
                table: "NDWTCapitalAdequacyReturns",
                newName: "NetSurplusAfterTax");

            migrationBuilder.RenameColumn(
                name: "InvestmentsInSubsidiaryAndEquityInstruments",
                table: "NDWTCapitalAdequacyReturns",
                newName: "MinimumRetainedEarningsToCoreCaptialRequirement");

            migrationBuilder.RenameColumn(
                name: "GeneralReserves",
                table: "NDWTCapitalAdequacyReturns",
                newName: "MinimumCoreCapitalToDepositsRequirement");

            migrationBuilder.RenameColumn(
                name: "Difference",
                table: "NDWTCapitalAdequacyReturns",
                newName: "InvestmentsInSubsidiary");

            migrationBuilder.RenameColumn(
                name: "DepositsAndBalancesAtOtherInstitutions",
                table: "NDWTCapitalAdequacyReturns",
                newName: "DifferenceInAssets");

            migrationBuilder.RenameColumn(
                name: "CashLocalAndForeignCurrency",
                table: "NDWTCapitalAdequacyReturns",
                newName: "DepositsBalancesAtOtherInstitutions");

            migrationBuilder.RenameColumn(
                name: "CapitalGrantsEquityInNature",
                table: "NDWTCapitalAdequacyReturns",
                newName: "CoreCapitalToDepositsRatio");

            migrationBuilder.RenameColumn(
                name: "AssetValueOffBalanceSheet",
                table: "NDWTCapitalAdequacyReturns",
                newName: "CoreCapitalToDepositsExcessDeficiency");

            migrationBuilder.AddColumn<decimal>(
                name: "CapitalGrants",
                table: "NDWTCapitalAdequacyReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CashLocalForeign",
                table: "NDWTCapitalAdequacyReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CoreCapitalToAssetsExcessDeficiency",
                table: "NDWTCapitalAdequacyReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CoreCapitalToAssetsRatio",
                table: "NDWTCapitalAdequacyReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapitalGrants",
                table: "NDWTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "CashLocalForeign",
                table: "NDWTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "CoreCapitalToAssetsExcessDeficiency",
                table: "NDWTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "CoreCapitalToAssetsRatio",
                table: "NDWTCapitalAdequacyReturns");

            migrationBuilder.RenameColumn(
                name: "TotalAssets",
                table: "NDWTCapitalAdequacyReturns",
                newName: "TotalOffBalanceSheetAssets");

            migrationBuilder.RenameColumn(
                name: "RetainedEarningsToCoreCaptialRatio",
                table: "NDWTCapitalAdequacyReturns",
                newName: "TotalAssetsOnBalanceSheet");

            migrationBuilder.RenameColumn(
                name: "RetainedEarningsToCoreCaptialExcessDeficiency",
                table: "NDWTCapitalAdequacyReturns",
                newName: "RetainedEarningsAccumulatedLosses");

            migrationBuilder.RenameColumn(
                name: "RetainedEarnings",
                table: "NDWTCapitalAdequacyReturns",
                newName: "NetSurplusAfterTaxCurrentYearToDate");

            migrationBuilder.RenameColumn(
                name: "OffBalanceSheetAssets",
                table: "NDWTCapitalAdequacyReturns",
                newName: "MinimumRetainedEarningsAndDisclosedReservesToAssetsRatio");

            migrationBuilder.RenameColumn(
                name: "NetSurplusAfterTax",
                table: "NDWTCapitalAdequacyReturns",
                newName: "MinimumCoreCapitalToDepositsRatio");

            migrationBuilder.RenameColumn(
                name: "MinimumRetainedEarningsToCoreCaptialRequirement",
                table: "NDWTCapitalAdequacyReturns",
                newName: "InvestmentsInSubsidiaryAndEquityInstruments");

            migrationBuilder.RenameColumn(
                name: "MinimumCoreCapitalToDepositsRequirement",
                table: "NDWTCapitalAdequacyReturns",
                newName: "GeneralReserves");

            migrationBuilder.RenameColumn(
                name: "InvestmentsInSubsidiary",
                table: "NDWTCapitalAdequacyReturns",
                newName: "Difference");

            migrationBuilder.RenameColumn(
                name: "DifferenceInAssets",
                table: "NDWTCapitalAdequacyReturns",
                newName: "DepositsAndBalancesAtOtherInstitutions");

            migrationBuilder.RenameColumn(
                name: "DepositsBalancesAtOtherInstitutions",
                table: "NDWTCapitalAdequacyReturns",
                newName: "CashLocalAndForeignCurrency");

            migrationBuilder.RenameColumn(
                name: "CoreCapitalToDepositsRatio",
                table: "NDWTCapitalAdequacyReturns",
                newName: "CapitalGrantsEquityInNature");

            migrationBuilder.RenameColumn(
                name: "CoreCapitalToDepositsExcessDeficiency",
                table: "NDWTCapitalAdequacyReturns",
                newName: "AssetValueOffBalanceSheet");
        }
    }
}
