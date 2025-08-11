using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class ADDrEUEiDfORSafet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyLiquidityReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DailyLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_ManagementReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "ManagementReturns");

            migrationBuilder.DropIndex(
                name: "IX_ManagementReturns_ReturnSubmissionId",
                table: "ManagementReturns");

            migrationBuilder.DropIndex(
                name: "IX_DailyLiquidityReturns_ReturnSubmissionId",
                table: "DailyLiquidityReturns");

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "SectoralLendingReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "SectoralLendingData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "OtherReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTDepositReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnSubmissionId",
                table: "ManagementReturns",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "ManagementReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "InsiderLendingHeaders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTRiskClassificationReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTLiquidityReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTInvestmentReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTFinancialPositionReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTComprehensiveIncomeReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DepositReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnSubmissionId",
                table: "DailyLiquidityReturns",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DailyLiquidityReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "AuditedRiskClassifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "AuditedFinancialPositions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "AuditedComprehensiveIncomes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManagementReturns_ReturnId",
                table: "ManagementReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyLiquidityReturns_ReturnId",
                table: "DailyLiquidityReturns",
                column: "ReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyLiquidityReturns_ReturnSubmissions_ReturnId",
                table: "DailyLiquidityReturns",
                column: "ReturnId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ManagementReturns_ReturnSubmissions_ReturnId",
                table: "ManagementReturns",
                column: "ReturnId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyLiquidityReturns_ReturnSubmissions_ReturnId",
                table: "DailyLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_ManagementReturns_ReturnSubmissions_ReturnId",
                table: "ManagementReturns");

            migrationBuilder.DropIndex(
                name: "IX_ManagementReturns_ReturnId",
                table: "ManagementReturns");

            migrationBuilder.DropIndex(
                name: "IX_DailyLiquidityReturns_ReturnId",
                table: "DailyLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "SectoralLendingData");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "OtherReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "ManagementReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "DTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "DTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "DailyLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "AuditedRiskClassifications");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "AuditedFinancialPositions");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "AuditedComprehensiveIncomes");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnSubmissionId",
                table: "ManagementReturns",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnSubmissionId",
                table: "DailyLiquidityReturns",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementReturns_ReturnSubmissionId",
                table: "ManagementReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyLiquidityReturns_ReturnSubmissionId",
                table: "DailyLiquidityReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyLiquidityReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DailyLiquidityReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManagementReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "ManagementReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
