using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class ToaReturnsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyLiquidityReturns_Returns_ReturnId",
                table: "DailyLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DepositReturns_Returns_ReturnId",
                table: "DepositReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTCapitalAdequacyReturns_Returns_ReturnId",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_Returns_ReturnId",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTFinancialPositionReturns_Returns_ReturnId",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTInvestmentReturns_Returns_ReturnId",
                table: "DTInvestmentReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTLiquidityReturns_Returns_ReturnId",
                table: "DTLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTRiskClassificationReturns_Returns_ReturnId",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_InsiderLendingHeaders_Returns_ReturnId",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_ManagementReturns_Returns_ReturnId",
                table: "ManagementReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NDWTLiquidityReturns_Returns_ReturnId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_Returns_ReturnId",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_Returns_ReturnId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTDepositReturns_Returns_ReturnId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTFinancialPositionReturns_Returns_ReturnId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTInvestmentReturns_Returns_ReturnId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTRiskClassificationReturns_Returns_ReturnId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherReturns_Returns_ReturnId",
                table: "OtherReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Returns_PreviousVersionId",
                table: "Returns");

            migrationBuilder.DropForeignKey(
                name: "FK_SaccoAnalysis_Returns_ReturnId",
                table: "SaccoAnalysis");

            migrationBuilder.DropForeignKey(
                name: "FK_SectoralLendingReports_Returns_ReturnId",
                table: "SectoralLendingReports");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Returns_ReturnId",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_ReturnId",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_SectoralLendingReports_ReturnId",
                table: "SectoralLendingReports");

            migrationBuilder.DropIndex(
                name: "IX_SaccoAnalysis_ReturnId",
                table: "SaccoAnalysis");

            migrationBuilder.DropIndex(
                name: "IX_Returns_PreviousVersionId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_OtherReturns_ReturnId",
                table: "OtherReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTRiskClassificationReturns_ReturnId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTInvestmentReturns_ReturnId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTFinancialPositionReturns_ReturnId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTDepositReturns_ReturnId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTComprehensiveIncomeReturns_ReturnId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTCapitalAdequacyReturns_ReturnId",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropIndex(
                name: "IX_NDWTLiquidityReturns_ReturnId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropIndex(
                name: "IX_ManagementReturns_ReturnId",
                table: "ManagementReturns");

            migrationBuilder.DropIndex(
                name: "IX_InsiderLendingHeaders_ReturnId",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropIndex(
                name: "IX_DTRiskClassificationReturns_ReturnId",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTLiquidityReturns_ReturnId",
                table: "DTLiquidityReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTInvestmentReturns_ReturnId",
                table: "DTInvestmentReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTFinancialPositionReturns_ReturnId",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTComprehensiveIncomeReturns_ReturnId",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTCapitalAdequacyReturns_ReturnId",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropIndex(
                name: "IX_DepositReturns_ReturnId",
                table: "DepositReturns");

            migrationBuilder.DropIndex(
                name: "IX_DailyLiquidityReturns_ReturnId",
                table: "DailyLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "SaccoAnalysis");

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

            migrationBuilder.AlterColumn<string>(
                name: "PreviousVersionId",
                table: "Returns",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "WorkflowInstances",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "SectoralLendingReports",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "SaccoAnalysis",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PreviousVersionId",
                table: "Returns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "OtherReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTDepositReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NWDTCapitalAdequacyReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "ManagementReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "InsiderLendingHeaders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTRiskClassificationReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTLiquidityReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTInvestmentReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTFinancialPositionReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTComprehensiveIncomeReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DTCapitalAdequacyReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DepositReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "DailyLiquidityReturns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_ReturnId",
                table: "WorkflowInstances",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_SectoralLendingReports_ReturnId",
                table: "SectoralLendingReports",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_SaccoAnalysis_ReturnId",
                table: "SaccoAnalysis",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_PreviousVersionId",
                table: "Returns",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_OtherReturns_ReturnId",
                table: "OtherReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTRiskClassificationReturns_ReturnId",
                table: "NWDTRiskClassificationReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTInvestmentReturns_ReturnId",
                table: "NWDTInvestmentReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTFinancialPositionReturns_ReturnId",
                table: "NWDTFinancialPositionReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTDepositReturns_ReturnId",
                table: "NWDTDepositReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTComprehensiveIncomeReturns_ReturnId",
                table: "NWDTComprehensiveIncomeReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTCapitalAdequacyReturns_ReturnId",
                table: "NWDTCapitalAdequacyReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_NDWTLiquidityReturns_ReturnId",
                table: "NDWTLiquidityReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementReturns_ReturnId",
                table: "ManagementReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_InsiderLendingHeaders_ReturnId",
                table: "InsiderLendingHeaders",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTRiskClassificationReturns_ReturnId",
                table: "DTRiskClassificationReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTLiquidityReturns_ReturnId",
                table: "DTLiquidityReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTInvestmentReturns_ReturnId",
                table: "DTInvestmentReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTFinancialPositionReturns_ReturnId",
                table: "DTFinancialPositionReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTComprehensiveIncomeReturns_ReturnId",
                table: "DTComprehensiveIncomeReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DTCapitalAdequacyReturns_ReturnId",
                table: "DTCapitalAdequacyReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositReturns_ReturnId",
                table: "DepositReturns",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyLiquidityReturns_ReturnId",
                table: "DailyLiquidityReturns",
                column: "ReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyLiquidityReturns_Returns_ReturnId",
                table: "DailyLiquidityReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DepositReturns_Returns_ReturnId",
                table: "DepositReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTCapitalAdequacyReturns_Returns_ReturnId",
                table: "DTCapitalAdequacyReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_Returns_ReturnId",
                table: "DTComprehensiveIncomeReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTFinancialPositionReturns_Returns_ReturnId",
                table: "DTFinancialPositionReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTInvestmentReturns_Returns_ReturnId",
                table: "DTInvestmentReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTLiquidityReturns_Returns_ReturnId",
                table: "DTLiquidityReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTRiskClassificationReturns_Returns_ReturnId",
                table: "DTRiskClassificationReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InsiderLendingHeaders_Returns_ReturnId",
                table: "InsiderLendingHeaders",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ManagementReturns_Returns_ReturnId",
                table: "ManagementReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NDWTLiquidityReturns_Returns_ReturnId",
                table: "NDWTLiquidityReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_Returns_ReturnId",
                table: "NWDTCapitalAdequacyReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_Returns_ReturnId",
                table: "NWDTComprehensiveIncomeReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTDepositReturns_Returns_ReturnId",
                table: "NWDTDepositReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTFinancialPositionReturns_Returns_ReturnId",
                table: "NWDTFinancialPositionReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTInvestmentReturns_Returns_ReturnId",
                table: "NWDTInvestmentReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTRiskClassificationReturns_Returns_ReturnId",
                table: "NWDTRiskClassificationReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OtherReturns_Returns_ReturnId",
                table: "OtherReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Returns_PreviousVersionId",
                table: "Returns",
                column: "PreviousVersionId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaccoAnalysis_Returns_ReturnId",
                table: "SaccoAnalysis",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SectoralLendingReports_Returns_ReturnId",
                table: "SectoralLendingReports",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Returns_ReturnId",
                table: "WorkflowInstances",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");
        }
    }
}
