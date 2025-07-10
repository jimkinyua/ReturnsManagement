using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnnecessasyFieldsNWDTReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NDWTLiquidityReturns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "NWDTRiskClassificationReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "NWDTDepositReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "NWDTDepositReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "NWDTDepositReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "NWDTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "NWDTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "NWDTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
