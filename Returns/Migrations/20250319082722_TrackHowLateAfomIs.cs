using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class TrackHowLateAfomIs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "StatementOfFinancialPositionReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "StatementOfFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "StatementOfComprehensiveIncomeReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "StatementOfComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "RiskClassifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "RiskClassifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "LiquidityReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "LiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "InvestmentReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "InvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "DepositReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DepositReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "CapitalAdequacies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "StatementOfFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "StatementOfFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "RiskClassifications");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "RiskClassifications");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "LiquidityReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "LiquidityReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "InvestmentReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "InvestmentReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "CapitalAdequacies");
        }
    }
}
