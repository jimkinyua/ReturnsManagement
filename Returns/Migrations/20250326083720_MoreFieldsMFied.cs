using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class MoreFieldsMFied : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "StatementOfFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "StatementOfComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "RiskClassifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "OtherReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "NWDTDepositReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "NDWTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "LiquidityReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "InvestmentReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "DepositReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "CapitalAdequacies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "StatementOfFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "RiskClassifications");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "OtherReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "NDWTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "LiquidityReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "InvestmentReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "CapitalAdequacies");
        }
    }
}
