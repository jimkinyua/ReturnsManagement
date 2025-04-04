using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreatedAtTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "StatementOfFinancialPositionReturns",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "StatementOfComprehensiveIncomeReturns",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "SaccoAnalysis",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "RiskClassifications",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "Returns",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "ReturnForms",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "QuarterDates",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "Periods",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "LiquidityReturns",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "InvestmentReturns",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "DepositReturns",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "CapitalAdequacies",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "StatementOfFinancialPositionReturns",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "StatementOfComprehensiveIncomeReturns",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "SaccoAnalysis",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "RiskClassifications",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Returns",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ReturnForms",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "QuarterDates",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Periods",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "LiquidityReturns",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "InvestmentReturns",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "DepositReturns",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "CapitalAdequacies",
                newName: "DateTime");
        }
    }
}
