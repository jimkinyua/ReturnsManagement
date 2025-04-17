using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RefineSectotralLendingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SectorData_EconomicSubSectors_SubSectorId",
                table: "SectorData");

            migrationBuilder.DropForeignKey(
                name: "FK_SectorData_SectoralLendingReports_ReportId",
                table: "SectorData");

            migrationBuilder.DropTable(
                name: "EconomicSubSectors");

            migrationBuilder.DropIndex(
                name: "IX_SectorData_ReportId",
                table: "SectorData");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "SectorData");

            migrationBuilder.DropColumn(
                name: "SubSectorCode",
                table: "SectorData");

            migrationBuilder.DropColumn(
                name: "SubSectorName",
                table: "SectorData");

            migrationBuilder.RenameColumn(
                name: "SubSectorId",
                table: "SectorData",
                newName: "SectoralLendingReportId");

            migrationBuilder.RenameIndex(
                name: "IX_SectorData_SubSectorId",
                table: "SectorData",
                newName: "IX_SectorData_SectoralLendingReportId");

            migrationBuilder.RenameColumn(
                name: "SectorName",
                table: "EconomicSectors",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "SectorCode",
                table: "EconomicSectors",
                newName: "Code");

            migrationBuilder.AddColumn<string>(
                name: "EconomicSectorId",
                table: "SectorData",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubCategoryId",
                table: "EconomicSectors",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SectorData_EconomicSectorId",
                table: "SectorData",
                column: "EconomicSectorId");

            migrationBuilder.CreateIndex(
                name: "IX_EconomicSectors_SubCategoryId",
                table: "EconomicSectors",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryId",
                table: "SubCategories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_EconomicSectors_SubCategories_SubCategoryId",
                table: "EconomicSectors",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SectorData_EconomicSectors_EconomicSectorId",
                table: "SectorData",
                column: "EconomicSectorId",
                principalTable: "EconomicSectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SectorData_SectoralLendingReports_SectoralLendingReportId",
                table: "SectorData",
                column: "SectoralLendingReportId",
                principalTable: "SectoralLendingReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EconomicSectors_SubCategories_SubCategoryId",
                table: "EconomicSectors");

            migrationBuilder.DropForeignKey(
                name: "FK_SectorData_EconomicSectors_EconomicSectorId",
                table: "SectorData");

            migrationBuilder.DropForeignKey(
                name: "FK_SectorData_SectoralLendingReports_SectoralLendingReportId",
                table: "SectorData");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_SectorData_EconomicSectorId",
                table: "SectorData");

            migrationBuilder.DropIndex(
                name: "IX_EconomicSectors_SubCategoryId",
                table: "EconomicSectors");

            migrationBuilder.DropColumn(
                name: "EconomicSectorId",
                table: "SectorData");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "EconomicSectors");

            migrationBuilder.RenameColumn(
                name: "SectoralLendingReportId",
                table: "SectorData",
                newName: "SubSectorId");

            migrationBuilder.RenameIndex(
                name: "IX_SectorData_SectoralLendingReportId",
                table: "SectorData",
                newName: "IX_SectorData_SubSectorId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "EconomicSectors",
                newName: "SectorName");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "EconomicSectors",
                newName: "SectorCode");

            migrationBuilder.AddColumn<string>(
                name: "ReportId",
                table: "SectorData",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubSectorCode",
                table: "SectorData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubSectorName",
                table: "SectorData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EconomicSubSectors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SectorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubSectorCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubSectorName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomicSubSectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EconomicSubSectors_EconomicSectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "EconomicSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SectorData_ReportId",
                table: "SectorData",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_EconomicSubSectors_SectorId",
                table: "EconomicSubSectors",
                column: "SectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SectorData_EconomicSubSectors_SubSectorId",
                table: "SectorData",
                column: "SubSectorId",
                principalTable: "EconomicSubSectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SectorData_SectoralLendingReports_ReportId",
                table: "SectorData",
                column: "ReportId",
                principalTable: "SectoralLendingReports",
                principalColumn: "Id");
        }
    }
}
