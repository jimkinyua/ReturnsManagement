using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class LinkReturnSubmissionToChildren : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "FK_SaccoAnalysis_Returns_ReturnId",
                table: "SaccoAnalysis");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "SaccoAnalysis",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "ReturnSubmissionId",
                table: "SaccoAnalysis",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId1",
                table: "SaccoAnalysis",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTDepositReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "NWDTDepositReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTCapitalAdequacyReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "NWDTCapitalAdequacyReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTRiskClassificationReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "DTRiskClassificationReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTLiquidityReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "DTLiquidityReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTInvestmentReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "DTInvestmentReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTFinancialPositionReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "DTFinancialPositionReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTComprehensiveIncomeReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "DTComprehensiveIncomeReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTCapitalAdequacyReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "DTCapitalAdequacyReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DepositReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "DepositReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ReturnSubmission",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpectedReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SaccoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnSubmission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnSubmission_ExpectedReturns_ExpectedReturnId",
                        column: x => x.ExpectedReturnId,
                        principalTable: "ExpectedReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaccoAnalysis_ReturnSubmissionId1",
                table: "SaccoAnalysis",
                column: "ReturnSubmissionId1");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTRiskClassificationReturns_ReturnSubmissionId",
                table: "NWDTRiskClassificationReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTInvestmentReturns_ReturnSubmissionId",
                table: "NWDTInvestmentReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTFinancialPositionReturns_ReturnSubmissionId",
                table: "NWDTFinancialPositionReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTDepositReturns_ReturnSubmissionId",
                table: "NWDTDepositReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTComprehensiveIncomeReturns_ReturnSubmissionId",
                table: "NWDTComprehensiveIncomeReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_NWDTCapitalAdequacyReturns_ReturnSubmissionId",
                table: "NWDTCapitalAdequacyReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_NDWTLiquidityReturns_ReturnSubmissionId",
                table: "NDWTLiquidityReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DTRiskClassificationReturns_ReturnSubmissionId",
                table: "DTRiskClassificationReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DTLiquidityReturns_ReturnSubmissionId",
                table: "DTLiquidityReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DTInvestmentReturns_ReturnSubmissionId",
                table: "DTInvestmentReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DTFinancialPositionReturns_ReturnSubmissionId",
                table: "DTFinancialPositionReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DTComprehensiveIncomeReturns_ReturnSubmissionId",
                table: "DTComprehensiveIncomeReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DTCapitalAdequacyReturns_ReturnSubmissionId",
                table: "DTCapitalAdequacyReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositReturns_ReturnSubmissionId",
                table: "DepositReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnSubmission_ExpectedReturnId",
                table: "ReturnSubmission",
                column: "ExpectedReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepositReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DepositReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DepositReturns_Returns_ReturnId",
                table: "DepositReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTCapitalAdequacyReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTCapitalAdequacyReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTCapitalAdequacyReturns_Returns_ReturnId",
                table: "DTCapitalAdequacyReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTComprehensiveIncomeReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_Returns_ReturnId",
                table: "DTComprehensiveIncomeReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTFinancialPositionReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTFinancialPositionReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTFinancialPositionReturns_Returns_ReturnId",
                table: "DTFinancialPositionReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTInvestmentReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTInvestmentReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTInvestmentReturns_Returns_ReturnId",
                table: "DTInvestmentReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTLiquidityReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTLiquidityReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTLiquidityReturns_Returns_ReturnId",
                table: "DTLiquidityReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DTRiskClassificationReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTRiskClassificationReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTRiskClassificationReturns_Returns_ReturnId",
                table: "DTRiskClassificationReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NDWTLiquidityReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NDWTLiquidityReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NDWTLiquidityReturns_Returns_ReturnId",
                table: "NDWTLiquidityReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTCapitalAdequacyReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_Returns_ReturnId",
                table: "NWDTCapitalAdequacyReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTComprehensiveIncomeReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_Returns_ReturnId",
                table: "NWDTComprehensiveIncomeReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTDepositReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTDepositReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTDepositReturns_Returns_ReturnId",
                table: "NWDTDepositReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTFinancialPositionReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTFinancialPositionReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTFinancialPositionReturns_Returns_ReturnId",
                table: "NWDTFinancialPositionReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTInvestmentReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTInvestmentReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTInvestmentReturns_Returns_ReturnId",
                table: "NWDTInvestmentReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTRiskClassificationReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTRiskClassificationReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTRiskClassificationReturns_Returns_ReturnId",
                table: "NWDTRiskClassificationReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaccoAnalysis_ReturnSubmission_ReturnSubmissionId1",
                table: "SaccoAnalysis",
                column: "ReturnSubmissionId1",
                principalTable: "ReturnSubmission",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaccoAnalysis_Returns_ReturnId",
                table: "SaccoAnalysis",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepositReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DepositReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DepositReturns_Returns_ReturnId",
                table: "DepositReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTCapitalAdequacyReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTCapitalAdequacyReturns_Returns_ReturnId",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_Returns_ReturnId",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTFinancialPositionReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTFinancialPositionReturns_Returns_ReturnId",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTInvestmentReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTInvestmentReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTInvestmentReturns_Returns_ReturnId",
                table: "DTInvestmentReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTLiquidityReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTLiquidityReturns_Returns_ReturnId",
                table: "DTLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTRiskClassificationReturns_ReturnSubmission_ReturnSubmissionId",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DTRiskClassificationReturns_Returns_ReturnId",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NDWTLiquidityReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NDWTLiquidityReturns_Returns_ReturnId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_Returns_ReturnId",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_Returns_ReturnId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTDepositReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTDepositReturns_Returns_ReturnId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTFinancialPositionReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTFinancialPositionReturns_Returns_ReturnId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTInvestmentReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTInvestmentReturns_Returns_ReturnId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTRiskClassificationReturns_ReturnSubmission_ReturnSubmissionId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_NWDTRiskClassificationReturns_Returns_ReturnId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SaccoAnalysis_ReturnSubmission_ReturnSubmissionId1",
                table: "SaccoAnalysis");

            migrationBuilder.DropForeignKey(
                name: "FK_SaccoAnalysis_Returns_ReturnId",
                table: "SaccoAnalysis");

            migrationBuilder.DropTable(
                name: "ReturnSubmission");

            migrationBuilder.DropIndex(
                name: "IX_SaccoAnalysis_ReturnSubmissionId1",
                table: "SaccoAnalysis");

            migrationBuilder.DropIndex(
                name: "IX_NWDTRiskClassificationReturns_ReturnSubmissionId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTInvestmentReturns_ReturnSubmissionId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTFinancialPositionReturns_ReturnSubmissionId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTDepositReturns_ReturnSubmissionId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTComprehensiveIncomeReturns_ReturnSubmissionId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropIndex(
                name: "IX_NWDTCapitalAdequacyReturns_ReturnSubmissionId",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropIndex(
                name: "IX_NDWTLiquidityReturns_ReturnSubmissionId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTRiskClassificationReturns_ReturnSubmissionId",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTLiquidityReturns_ReturnSubmissionId",
                table: "DTLiquidityReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTInvestmentReturns_ReturnSubmissionId",
                table: "DTInvestmentReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTFinancialPositionReturns_ReturnSubmissionId",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTComprehensiveIncomeReturns_ReturnSubmissionId",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropIndex(
                name: "IX_DTCapitalAdequacyReturns_ReturnSubmissionId",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropIndex(
                name: "IX_DepositReturns_ReturnSubmissionId",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "SaccoAnalysis");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId1",
                table: "SaccoAnalysis");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "DTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "DTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "DTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "DTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "DTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "DepositReturns");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "SaccoAnalysis",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTRiskClassificationReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTInvestmentReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTFinancialPositionReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTDepositReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTComprehensiveIncomeReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NWDTCapitalAdequacyReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "NDWTLiquidityReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTRiskClassificationReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTLiquidityReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTInvestmentReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTFinancialPositionReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTComprehensiveIncomeReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DTCapitalAdequacyReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DepositReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DepositReturns_Returns_ReturnId",
                table: "DepositReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTCapitalAdequacyReturns_Returns_ReturnId",
                table: "DTCapitalAdequacyReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTComprehensiveIncomeReturns_Returns_ReturnId",
                table: "DTComprehensiveIncomeReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTFinancialPositionReturns_Returns_ReturnId",
                table: "DTFinancialPositionReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTInvestmentReturns_Returns_ReturnId",
                table: "DTInvestmentReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTLiquidityReturns_Returns_ReturnId",
                table: "DTLiquidityReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DTRiskClassificationReturns_Returns_ReturnId",
                table: "DTRiskClassificationReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NDWTLiquidityReturns_Returns_ReturnId",
                table: "NDWTLiquidityReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTCapitalAdequacyReturns_Returns_ReturnId",
                table: "NWDTCapitalAdequacyReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTComprehensiveIncomeReturns_Returns_ReturnId",
                table: "NWDTComprehensiveIncomeReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTDepositReturns_Returns_ReturnId",
                table: "NWDTDepositReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTFinancialPositionReturns_Returns_ReturnId",
                table: "NWDTFinancialPositionReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTInvestmentReturns_Returns_ReturnId",
                table: "NWDTInvestmentReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NWDTRiskClassificationReturns_Returns_ReturnId",
                table: "NWDTRiskClassificationReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaccoAnalysis_Returns_ReturnId",
                table: "SaccoAnalysis",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
