using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class CalcFieldsToBeStoredAsWellIn2B : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LiquidityRatio",
                table: "NDWTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LiquidityRatioExcessDeficit",
                table: "NDWTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetBankBalances",
                table: "NDWTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetFinancialInstitutionBalances",
                table: "NDWTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetLiquidAssets",
                table: "NDWTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBankBalances",
                table: "NDWTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGovernmentSecurities",
                table: "NDWTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNotesAndCoins",
                table: "NDWTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalOtherFinancialInstitutions",
                table: "NDWTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalOtherLiabilities",
                table: "NDWTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LiquidityRatio",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "LiquidityRatioExcessDeficit",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "NetBankBalances",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "NetFinancialInstitutionBalances",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "NetLiquidAssets",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "TotalBankBalances",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "TotalGovernmentSecurities",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "TotalNotesAndCoins",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "TotalOtherFinancialInstitutions",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "TotalOtherLiabilities",
                table: "NDWTLiquidityReturns");
        }
    }
}
