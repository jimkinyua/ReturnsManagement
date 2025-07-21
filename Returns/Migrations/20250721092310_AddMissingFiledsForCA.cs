using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFiledsForCA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashLocalForeign",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.RenameColumn(
                name: "TotalDepositsLiabilitiesAsPerBalanceSheet",
                table: "NWDTCapitalAdequacyReturns",
                newName: "TotalOffBalanceSheetAssets");

            migrationBuilder.RenameColumn(
                name: "TotalDepositsLiabilities",
                table: "NWDTCapitalAdequacyReturns",
                newName: "TotalDepositsLiabilitiesPerBalanceSheet");

            migrationBuilder.RenameColumn(
                name: "TotalAssetValueOffBalanceSheet",
                table: "NWDTCapitalAdequacyReturns",
                newName: "RetainedEarningsAndDisclosedReservesToCoreCapitalExcessDeficiency");

            migrationBuilder.RenameColumn(
                name: "RetainedEarningsToCoreCaptialRatio",
                table: "NWDTCapitalAdequacyReturns",
                newName: "RetainedEarningsAccumulatedLosses");

            migrationBuilder.RenameColumn(
                name: "RetainedEarningsToCoreCaptialExcessDeficiency",
                table: "NWDTCapitalAdequacyReturns",
                newName: "NetSurplusAfterTaxCurrentYearToDate");

            migrationBuilder.RenameColumn(
                name: "RetainedEarnings",
                table: "NWDTCapitalAdequacyReturns",
                newName: "MinimumRetainedEarningsAndDisclosedReservesToCoreCapitalRequirement");

            migrationBuilder.RenameColumn(
                name: "OffBalanceSheetAssets",
                table: "NWDTCapitalAdequacyReturns",
                newName: "MinimumCoreCapitalToDepositsRatioRequirement");

            migrationBuilder.RenameColumn(
                name: "NetSurplusAfterTax",
                table: "NWDTCapitalAdequacyReturns",
                newName: "MinimumCoreCapitalToAssetsRatioRequirement");

            migrationBuilder.RenameColumn(
                name: "MinimumRetainedEarningsToCoreCaptialRequirement",
                table: "NWDTCapitalAdequacyReturns",
                newName: "InvestmentsInSubsidiaryAndEquityInstruments");

            migrationBuilder.RenameColumn(
                name: "MinimumCoreCapitalToDepositsRequirement",
                table: "NWDTCapitalAdequacyReturns",
                newName: "GeneralReserves");

            migrationBuilder.RenameColumn(
                name: "MinimumCoreCapitalToAssetsRatio",
                table: "NWDTCapitalAdequacyReturns",
                newName: "Difference");

            migrationBuilder.RenameColumn(
                name: "InvestmentsInSubsidiary",
                table: "NWDTCapitalAdequacyReturns",
                newName: "DepositsAndBalancesAtOtherInstitutions");

            migrationBuilder.RenameColumn(
                name: "DifferenceInAssets",
                table: "NWDTCapitalAdequacyReturns",
                newName: "CoreCapitalToDepositsRatioExcessDeficiency");

            migrationBuilder.RenameColumn(
                name: "DepositsBalancesAtOtherInstitutions",
                table: "NWDTCapitalAdequacyReturns",
                newName: "CoreCapitalToAssetsRatioExcessDeficiency");

            migrationBuilder.RenameColumn(
                name: "CoreCapitalToDepositsExcessDeficiency",
                table: "NWDTCapitalAdequacyReturns",
                newName: "CashLocalAndForeignCurrency");

            migrationBuilder.RenameColumn(
                name: "CoreCapitalToAssetsExcessDeficiency",
                table: "NWDTCapitalAdequacyReturns",
                newName: "CapitalGrantsEquityInNature");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalOffBalanceSheetAssets",
                table: "NWDTCapitalAdequacyReturns",
                newName: "TotalDepositsLiabilitiesAsPerBalanceSheet");

            migrationBuilder.RenameColumn(
                name: "TotalDepositsLiabilitiesPerBalanceSheet",
                table: "NWDTCapitalAdequacyReturns",
                newName: "TotalDepositsLiabilities");

            migrationBuilder.RenameColumn(
                name: "RetainedEarningsAndDisclosedReservesToCoreCapitalExcessDeficiency",
                table: "NWDTCapitalAdequacyReturns",
                newName: "TotalAssetValueOffBalanceSheet");

            migrationBuilder.RenameColumn(
                name: "RetainedEarningsAccumulatedLosses",
                table: "NWDTCapitalAdequacyReturns",
                newName: "RetainedEarningsToCoreCaptialRatio");

            migrationBuilder.RenameColumn(
                name: "NetSurplusAfterTaxCurrentYearToDate",
                table: "NWDTCapitalAdequacyReturns",
                newName: "RetainedEarningsToCoreCaptialExcessDeficiency");

            migrationBuilder.RenameColumn(
                name: "MinimumRetainedEarningsAndDisclosedReservesToCoreCapitalRequirement",
                table: "NWDTCapitalAdequacyReturns",
                newName: "RetainedEarnings");

            migrationBuilder.RenameColumn(
                name: "MinimumCoreCapitalToDepositsRatioRequirement",
                table: "NWDTCapitalAdequacyReturns",
                newName: "OffBalanceSheetAssets");

            migrationBuilder.RenameColumn(
                name: "MinimumCoreCapitalToAssetsRatioRequirement",
                table: "NWDTCapitalAdequacyReturns",
                newName: "NetSurplusAfterTax");

            migrationBuilder.RenameColumn(
                name: "InvestmentsInSubsidiaryAndEquityInstruments",
                table: "NWDTCapitalAdequacyReturns",
                newName: "MinimumRetainedEarningsToCoreCaptialRequirement");

            migrationBuilder.RenameColumn(
                name: "GeneralReserves",
                table: "NWDTCapitalAdequacyReturns",
                newName: "MinimumCoreCapitalToDepositsRequirement");

            migrationBuilder.RenameColumn(
                name: "Difference",
                table: "NWDTCapitalAdequacyReturns",
                newName: "MinimumCoreCapitalToAssetsRatio");

            migrationBuilder.RenameColumn(
                name: "DepositsAndBalancesAtOtherInstitutions",
                table: "NWDTCapitalAdequacyReturns",
                newName: "InvestmentsInSubsidiary");

            migrationBuilder.RenameColumn(
                name: "CoreCapitalToDepositsRatioExcessDeficiency",
                table: "NWDTCapitalAdequacyReturns",
                newName: "DifferenceInAssets");

            migrationBuilder.RenameColumn(
                name: "CoreCapitalToAssetsRatioExcessDeficiency",
                table: "NWDTCapitalAdequacyReturns",
                newName: "DepositsBalancesAtOtherInstitutions");

            migrationBuilder.RenameColumn(
                name: "CashLocalAndForeignCurrency",
                table: "NWDTCapitalAdequacyReturns",
                newName: "CoreCapitalToDepositsExcessDeficiency");

            migrationBuilder.RenameColumn(
                name: "CapitalGrantsEquityInNature",
                table: "NWDTCapitalAdequacyReturns",
                newName: "CoreCapitalToAssetsExcessDeficiency");

            migrationBuilder.AddColumn<decimal>(
                name: "CashLocalForeign",
                table: "NWDTCapitalAdequacyReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
