using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalFiledsForAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "SectoralLendingReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "SectoralLendingData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoType",
                table: "SectoralLendingData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "OtherReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "NWDTDepositReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "NWDTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "InsiderLoans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "InsiderLendingHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "DTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "DTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "DTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "DTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "DTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "DTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "DepositReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoCsNumber",
                table: "DailyLiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "SectoralLendingData");

            migrationBuilder.DropColumn(
                name: "SaccoType",
                table: "SectoralLendingData");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "OtherReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "InsiderLoans");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "DTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "DTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "SaccoCsNumber",
                table: "DailyLiquidityReturns");
        }
    }
}
