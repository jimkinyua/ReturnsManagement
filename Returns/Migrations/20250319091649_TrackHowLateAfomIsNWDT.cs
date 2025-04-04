using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class TrackHowLateAfomIsNWDT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "NWDTInvestmentReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "NWDTFinancialPositionReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "NWDTDepositReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "NWDTDepositReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "NWDTComprehensiveIncomeReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "NDWTLiquidityReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "NDWTCapitalAdequacyReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "NDWTCapitalAdequacyReturns");
        }
    }
}
