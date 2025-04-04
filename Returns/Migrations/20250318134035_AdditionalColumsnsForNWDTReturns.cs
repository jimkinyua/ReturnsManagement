using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalColumsnsForNWDTReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Frequency",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "NDWTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "NDWTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "NDWTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "NDWTCapitalAdequacyReturns");
        }
    }
}
