using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RefactorReturnForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnForms_ReturnPeriods_PeriodId",
                table: "ReturnForms");

            migrationBuilder.DropIndex(
                name: "IX_ReturnForms_PeriodId",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsCapitalAdequencyForm",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsDailyLiquidity",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsDepositReturnForm",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsFinancialPosition",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsInsiderLending",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsInvestmentReturn",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsLiquidityStatement",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsManagement",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsOtherForm",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsRiskClassification",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "IsSectoralLending",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "PeriodId",
                table: "ReturnForms");

            migrationBuilder.RenameColumn(
                name: "IsStatementOfComprehensiveIncome",
                table: "ReturnForms",
                newName: "IsActive");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "ReturnForms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReturnPeriodsId",
                table: "ReturnForms",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExpectedReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PeriodId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReturnFormId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FilingDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpectedReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpectedReturns_ReturnForms_ReturnFormId",
                        column: x => x.ReturnFormId,
                        principalTable: "ReturnForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpectedReturns_ReturnPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "ReturnPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnForms_ReturnPeriodsId",
                table: "ReturnForms",
                column: "ReturnPeriodsId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpectedReturns_PeriodId",
                table: "ExpectedReturns",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpectedReturns_ReturnFormId",
                table: "ExpectedReturns",
                column: "ReturnFormId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnForms_ReturnPeriods_ReturnPeriodsId",
                table: "ReturnForms",
                column: "ReturnPeriodsId",
                principalTable: "ReturnPeriods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnForms_ReturnPeriods_ReturnPeriodsId",
                table: "ReturnForms");

            migrationBuilder.DropTable(
                name: "ExpectedReturns");

            migrationBuilder.DropIndex(
                name: "IX_ReturnForms_ReturnPeriodsId",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "ReturnPeriodsId",
                table: "ReturnForms");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "ReturnForms",
                newName: "IsStatementOfComprehensiveIncome");

            migrationBuilder.AddColumn<bool>(
                name: "IsCapitalAdequencyForm",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDailyLiquidity",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDepositReturnForm",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinancialPosition",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInsiderLending",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInvestmentReturn",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLiquidityStatement",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsManagement",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOtherForm",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRiskClassification",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSectoralLending",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PeriodId",
                table: "ReturnForms",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnForms_PeriodId",
                table: "ReturnForms",
                column: "PeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnForms_ReturnPeriods_PeriodId",
                table: "ReturnForms",
                column: "PeriodId",
                principalTable: "ReturnPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
