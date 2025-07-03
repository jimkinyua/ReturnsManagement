using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class PeriodSetUpAndYears : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnForms_Periods_PeriodId",
                table: "ReturnForms");

            migrationBuilder.DropTable(
                name: "Periods");

            migrationBuilder.CreateTable(
                name: "FrequencyCatalogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IntervalDays = table.Column<int>(type: "int", nullable: false),
                    DefaultDeadlineOffset = table.Column<int>(type: "int", nullable: false),
                    LabelStrategy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrequencyCatalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportingYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnPeriods",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    YearId = table.Column<int>(type: "int", nullable: false),
                    FrequencyId = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilingDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnPeriods_FrequencyCatalogs_FrequencyId",
                        column: x => x.FrequencyId,
                        principalTable: "FrequencyCatalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnPeriods_ReportingYears_YearId",
                        column: x => x.YearId,
                        principalTable: "ReportingYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FrequencyCatalogs_Code",
                table: "FrequencyCatalogs",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FrequencyCatalogs_IsActive",
                table: "FrequencyCatalogs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingYears_Year",
                table: "ReportingYears",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnPeriods_FrequencyId_StartDate",
                table: "ReturnPeriods",
                columns: new[] { "FrequencyId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnPeriods_YearId_FrequencyId_SequenceNo",
                table: "ReturnPeriods",
                columns: new[] { "YearId", "FrequencyId", "SequenceNo" },
                unique: true,
                filter: "[SequenceNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnPeriods_YearId_Name",
                table: "ReturnPeriods",
                columns: new[] { "YearId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnForms_ReturnPeriods_PeriodId",
                table: "ReturnForms",
                column: "PeriodId",
                principalTable: "ReturnPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnForms_ReturnPeriods_PeriodId",
                table: "ReturnForms");

            migrationBuilder.DropTable(
                name: "ReturnPeriods");

            migrationBuilder.DropTable(
                name: "FrequencyCatalogs");

            migrationBuilder.DropTable(
                name: "ReportingYears");

            migrationBuilder.CreateTable(
                name: "Periods",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periods", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnForms_Periods_PeriodId",
                table: "ReturnForms",
                column: "PeriodId",
                principalTable: "Periods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
