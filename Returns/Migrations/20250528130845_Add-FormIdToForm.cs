using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddFormIdToForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "SectoralLendingReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "SectoralLendingData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "OtherReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "NWDTDepositReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "NWDTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "ManagementReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "InsiderLoans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "InsiderLendingHeaders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "DTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "DTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "DTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "DTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "DTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "DTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "DepositReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormId",
                table: "DailyLiquidityReturns",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormId",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "SectoralLendingData");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "OtherReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "ManagementReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "InsiderLoans");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "DTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "DTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "DailyLiquidityReturns");
        }
    }
}
