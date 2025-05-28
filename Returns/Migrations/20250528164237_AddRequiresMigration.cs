using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiresMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "SectoralLendingReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "SectoralLendingData",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "OtherReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "NWDTRiskClassificationReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "NWDTInvestmentReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "NWDTFinancialPositionReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "NWDTDepositReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "NWDTComprehensiveIncomeReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "NWDTCapitalAdequacyReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "NDWTLiquidityReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "ManagementReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "InsiderLoans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "InsiderLendingHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "DTRiskClassificationReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "DTLiquidityReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "DTInvestmentReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "DTFinancialPositionReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "DTComprehensiveIncomeReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "DTCapitalAdequacyReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "DepositReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresResubmission",
                table: "DailyLiquidityReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "SectoralLendingData");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "OtherReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "ManagementReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "InsiderLoans");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "DTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "DTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "RequiresResubmission",
                table: "DailyLiquidityReturns");
        }
    }
}
