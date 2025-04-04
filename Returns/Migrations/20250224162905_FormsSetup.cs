using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class FormsSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Periods",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsQuaterly = table.Column<bool>(type: "bit", nullable: false),
                    DeadlineDay = table.Column<int>(type: "int", nullable: false),
                    DeadlineMonth = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuarterDates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartMonth = table.Column<int>(type: "int", nullable: false),
                    StartDay = table.Column<int>(type: "int", nullable: false),
                    EndMonth = table.Column<int>(type: "int", nullable: false),
                    EndDay = table.Column<int>(type: "int", nullable: false),
                    DeadlineMonth = table.Column<int>(type: "int", nullable: false),
                    DeadlineDay = table.Column<int>(type: "int", nullable: false),
                    PeriodId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuarterDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuarterDates_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReturnForms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FormName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaccoTypeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCapitalAdequencyForm = table.Column<bool>(type: "bit", nullable: false),
                    IsLiquidityStatement = table.Column<bool>(type: "bit", nullable: false),
                    IsRiskClassification = table.Column<bool>(type: "bit", nullable: false),
                    IsInvestmentReturn = table.Column<bool>(type: "bit", nullable: false),
                    IsFinancialPosition = table.Column<bool>(type: "bit", nullable: false),
                    IsStatementOfComprehensiveIncome = table.Column<bool>(type: "bit", nullable: false),
                    IsDepositReturnForm = table.Column<bool>(type: "bit", nullable: false),
                    PeriodId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnForms_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuarterDates_PeriodId",
                table: "QuarterDates",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnForms_PeriodId",
                table: "ReturnForms",
                column: "PeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuarterDates");

            migrationBuilder.DropTable(
                name: "ReturnForms");

            migrationBuilder.DropTable(
                name: "Periods");
        }
    }
}
