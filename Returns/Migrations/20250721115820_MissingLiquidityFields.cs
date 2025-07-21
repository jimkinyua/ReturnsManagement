using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class MissingLiquidityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumRequirement",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "NetBankBalances",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "NetFinancialInstitutionBalances",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "TotalBankBalances",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "TotalGovernmentSecurities",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "NetBankBalances",
                table: "DTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "NetFinancialInstitutionBalances",
                table: "DTLiquidityReturns");

            migrationBuilder.RenameColumn(
                name: "TotalOtherFinancialInstitutions",
                table: "NDWTLiquidityReturns",
                newName: "TotalShortTermLiabilities");

            migrationBuilder.RenameColumn(
                name: "TotalNotesAndCoins",
                table: "NDWTLiquidityReturns",
                newName: "MinimumLiquidityRequirement");

            migrationBuilder.RenameColumn(
                name: "TotalGovernmentSecurities",
                table: "DTLiquidityReturns",
                newName: "NetLiquidAssetsToShortTermLiabilities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalShortTermLiabilities",
                table: "NDWTLiquidityReturns",
                newName: "TotalOtherFinancialInstitutions");

            migrationBuilder.RenameColumn(
                name: "MinimumLiquidityRequirement",
                table: "NDWTLiquidityReturns",
                newName: "TotalNotesAndCoins");

            migrationBuilder.RenameColumn(
                name: "NetLiquidAssetsToShortTermLiabilities",
                table: "DTLiquidityReturns",
                newName: "TotalGovernmentSecurities");

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumRequirement",
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
                name: "NetBankBalances",
                table: "DTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetFinancialInstitutionBalances",
                table: "DTLiquidityReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
