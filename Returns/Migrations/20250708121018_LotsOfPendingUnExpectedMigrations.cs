using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class LotsOfPendingUnExpectedMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepositReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DepositReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTCapitalAdequacyReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTFinancialPositionReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTInvestmentReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTInvestmentReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTLiquidityReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTRiskClassificationReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NDWTLiquidityReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTDepositReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTFinancialPositionReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTInvestmentReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTRiskClassificationReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnSubmission_ExpectedReturns_ExpectedReturnId",
                table: "ReturnSubmission");

            migrationBuilder.DropForeignKey(
                name: "FK_SaccoAnalysis_ReturnSubmission_ReturnSubmissionId1",
                table: "SaccoAnalysis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReturnSubmission",
                table: "ReturnSubmission");

            migrationBuilder.RenameTable(
                name: "ReturnSubmission",
                newName: "ReturnSubmissions");

            migrationBuilder.RenameIndex(
                name: "IX_ReturnSubmission_ExpectedReturnId",
                table: "ReturnSubmissions",
                newName: "IX_ReturnSubmissions_ExpectedReturnId");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnSubmissionId1",
                table: "SaccoAnalysis",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReturnSubmissions",
                table: "ReturnSubmissions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DepositReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DepositReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTCapitalAdequacyReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTCapitalAdequacyReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTComprehensiveIncomeReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTFinancialPositionReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTFinancialPositionReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTInvestmentReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTInvestmentReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTLiquidityReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTLiquidityReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTRiskClassificationReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTRiskClassificationReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NDWTLiquidityReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NDWTLiquidityReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTCapitalAdequacyReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTComprehensiveIncomeReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTDepositReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTDepositReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTFinancialPositionReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTFinancialPositionReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTInvestmentReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTInvestmentReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTRiskClassificationReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTRiskClassificationReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnSubmissions_ExpectedReturns_ExpectedReturnId",
                table: "ReturnSubmissions",
                column: "ExpectedReturnId",
                principalTable: "ExpectedReturns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaccoAnalysis_ReturnSubmissions_ReturnSubmissionId1",
                table: "SaccoAnalysis",
                column: "ReturnSubmissionId1",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepositReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DepositReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTCapitalAdequacyReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTFinancialPositionReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTInvestmentReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTInvestmentReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTLiquidityReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTRiskClassificationReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NDWTLiquidityReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTDepositReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTFinancialPositionReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTInvestmentReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTRiskClassificationReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnSubmissions_ExpectedReturns_ExpectedReturnId",
                table: "ReturnSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_SaccoAnalysis_ReturnSubmissions_ReturnSubmissionId1",
                table: "SaccoAnalysis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReturnSubmissions",
                table: "ReturnSubmissions");

            migrationBuilder.RenameTable(
                name: "ReturnSubmissions",
                newName: "ReturnSubmission");

            migrationBuilder.RenameIndex(
                name: "IX_ReturnSubmissions_ExpectedReturnId",
                table: "ReturnSubmission",
                newName: "IX_ReturnSubmission_ExpectedReturnId");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnSubmissionId1",
                table: "SaccoAnalysis",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReturnSubmission",
                table: "ReturnSubmission",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DepositReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DepositReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTCapitalAdequacyReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTCapitalAdequacyReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTComprehensiveIncomeReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTFinancialPositionReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTFinancialPositionReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTInvestmentReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTInvestmentReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTLiquidityReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTLiquidityReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTRiskClassificationReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTRiskClassificationReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NDWTLiquidityReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NDWTLiquidityReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTCapitalAdequacyReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTComprehensiveIncomeReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTDepositReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTDepositReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTFinancialPositionReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTFinancialPositionReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTInvestmentReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTInvestmentReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTRiskClassificationReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTRiskClassificationReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnSubmission_ExpectedReturns_ExpectedReturnId",
                table: "ReturnSubmission",
                column: "ExpectedReturnId",
                principalTable: "ExpectedReturns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaccoAnalysis_ReturnSubmission_ReturnSubmissionId1",
                table: "SaccoAnalysis",
                column: "ReturnSubmissionId1",
                principalTable: "ReturnSubmission",
                principalColumn: "Id");
        }
    }
}
