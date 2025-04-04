using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AdditonalReturnsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "StatementOfFinancialPositionReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "StatementOfFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "StatementOfFinancialPositionReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "StatementOfFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "StatementOfComprehensiveIncomeReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "StatementOfComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "StatementOfComprehensiveIncomeReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "StatementOfComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "RiskClassifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "RiskClassifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "RiskClassifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "RiskClassifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "LiquidityReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "LiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "LiquidityReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "LiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "InvestmentReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "InvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "InvestmentReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "InvestmentReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "DepositReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "DepositReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "DepositReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "DepositReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "CapitalAdequacies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "CapitalAdequacies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "CapitalAdequacies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "CapitalAdequacies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "StatementOfFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "StatementOfFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "StatementOfFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "StatementOfFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "RiskClassifications");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "RiskClassifications");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "RiskClassifications");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "RiskClassifications");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "LiquidityReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "LiquidityReturns");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "LiquidityReturns");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "LiquidityReturns");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "InvestmentReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "InvestmentReturns");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "InvestmentReturns");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "InvestmentReturns");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "CapitalAdequacies");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "CapitalAdequacies");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "CapitalAdequacies");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "CapitalAdequacies");
        }
    }
}
