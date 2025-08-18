using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalColumnsComprehensiveIncome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Taxes",
                table: "DTComprehensiveIncomeReturns",
                newName: "TotalOperatingExpenses");

            migrationBuilder.AddColumn<decimal>(
                name: "NetFinancialIncomeOrLoss",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetIncomeAfterTaxes",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetIncomeAfterTaxesAndDonations",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetIncomeBeforeTaxesAndDonations",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetNonOperatingIncome",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetOperatingIncome",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxesPayable",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFinancialExpense",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFinancialIncome",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFinancialIncomeFromInvestments",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFinancialIncomeFromLoans",
                table: "DTComprehensiveIncomeReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetFinancialIncomeOrLoss",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "NetIncomeAfterTaxes",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "NetIncomeAfterTaxesAndDonations",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "NetIncomeBeforeTaxesAndDonations",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "NetNonOperatingIncome",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "NetOperatingIncome",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "TaxesPayable",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "TotalFinancialExpense",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "TotalFinancialIncome",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "TotalFinancialIncomeFromInvestments",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "TotalFinancialIncomeFromLoans",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.RenameColumn(
                name: "TotalOperatingExpenses",
                table: "DTComprehensiveIncomeReturns",
                newName: "Taxes");
        }
    }
}
