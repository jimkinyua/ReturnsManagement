using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSectoralLendingProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSectoralLending",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EconomicSectors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SectorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SectorCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomicSectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SectoralLendingReports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SaccoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaccoName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectoralLendingReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EconomicSubSectors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubSectorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubSectorCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SectorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "SectorData",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubSectorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubSectorCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubSectorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReportId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectorData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectorData_EconomicSubSectors_SubSectorId",
                        column: x => x.SubSectorId,
                        principalTable: "EconomicSubSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectorData_SectoralLendingReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "SectoralLendingReports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EconomicSubSectors_SectorId",
                table: "EconomicSubSectors",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_SectorData_ReportId",
                table: "SectorData",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_SectorData_SubSectorId",
                table: "SectorData",
                column: "SubSectorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SectorData");

            migrationBuilder.DropTable(
                name: "EconomicSubSectors");

            migrationBuilder.DropTable(
                name: "SectoralLendingReports");

            migrationBuilder.DropTable(
                name: "EconomicSectors");

            migrationBuilder.DropColumn(
                name: "IsSectoralLending",
                table: "ReturnForms");
        }
    }
}
