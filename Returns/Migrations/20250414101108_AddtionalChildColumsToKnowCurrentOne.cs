using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddtionalChildColumsToKnowCurrentOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "StatementOfFinancialPositionReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "StatementOfComprehensiveIncomeReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "RiskClassifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "OtherReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "NWDTRiskClassificationReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "NWDTInvestmentReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "NWDTFinancialPositionReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "NWDTDepositReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "NWDTComprehensiveIncomeReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "NDWTLiquidityReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "NDWTCapitalAdequacyReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "LiquidityReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "InvestmentReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "DepositReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "CapitalAdequacies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "StatementOfFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "RiskClassifications");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "OtherReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "NDWTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "LiquidityReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "InvestmentReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "CapitalAdequacies");
        }
    }
}
