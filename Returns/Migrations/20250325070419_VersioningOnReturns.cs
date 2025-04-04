using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class VersioningOnReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "StatementOfFinancialPositionReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "StatementOfComprehensiveIncomeReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "RiskClassifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AmendmentDate",
                table: "Returns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveVersion",
                table: "Returns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreviousVersionId",
                table: "Returns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersionNumber",
                table: "Returns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "OtherReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "NWDTRiskClassificationReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "NWDTInvestmentReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "NWDTFinancialPositionReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "NWDTDepositReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "NWDTComprehensiveIncomeReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "NDWTLiquidityReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "NDWTCapitalAdequacyReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "LiquidityReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "InvestmentReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "DepositReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "CapitalAdequacies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Returns_PreviousVersionId",
                table: "Returns",
                column: "PreviousVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Returns_PreviousVersionId",
                table: "Returns",
                column: "PreviousVersionId",
                principalTable: "Returns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Returns_PreviousVersionId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_PreviousVersionId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "StatementOfFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "StatementOfComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "RiskClassifications");

            migrationBuilder.DropColumn(
                name: "AmendmentDate",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "IsActiveVersion",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "OtherReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "NWDTRiskClassificationReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "NWDTInvestmentReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "NWDTFinancialPositionReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "NWDTDepositReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "NWDTComprehensiveIncomeReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "NDWTLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "NDWTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "LiquidityReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "InvestmentReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "DepositReturns");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "CapitalAdequacies");
        }
    }
}
