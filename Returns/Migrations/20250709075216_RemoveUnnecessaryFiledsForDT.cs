using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnnecessaryFiledsForDT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "DTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "DTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "DTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "DTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "DepositReturns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "DTRiskClassificationReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "DTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "DTLiquidityReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "DTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "DTInvestmentReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "DTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "DTFinancialPositionReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "DTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "DTComprehensiveIncomeReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "DTComprehensiveIncomeReturns",
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

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "DepositReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
