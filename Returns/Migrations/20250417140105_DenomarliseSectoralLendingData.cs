using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class DenomarliseSectoralLendingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SectorData_EconomicSectors_EconomicSectorId",
                table: "SectorData");

            migrationBuilder.DropForeignKey(
                name: "FK_SectorData_SectoralLendingReports_SectoralLendingReportId",
                table: "SectorData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SectorData",
                table: "SectorData");

            migrationBuilder.RenameTable(
                name: "SectorData",
                newName: "SectoralLendingData");

            migrationBuilder.RenameIndex(
                name: "IX_SectorData_SectoralLendingReportId",
                table: "SectoralLendingData",
                newName: "IX_SectoralLendingData_SectoralLendingReportId");

            migrationBuilder.RenameIndex(
                name: "IX_SectorData_EconomicSectorId",
                table: "SectoralLendingData",
                newName: "IX_SectoralLendingData_EconomicSectorId");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "SectoralLendingData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EconomicSectorName",
                table: "SectoralLendingData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "SectoralLendingData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SectoralLendingData",
                table: "SectoralLendingData",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SectoralLendingData_EconomicSectors_EconomicSectorId",
                table: "SectoralLendingData",
                column: "EconomicSectorId",
                principalTable: "EconomicSectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SectoralLendingData_SectoralLendingReports_SectoralLendingReportId",
                table: "SectoralLendingData",
                column: "SectoralLendingReportId",
                principalTable: "SectoralLendingReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SectoralLendingData_EconomicSectors_EconomicSectorId",
                table: "SectoralLendingData");

            migrationBuilder.DropForeignKey(
                name: "FK_SectoralLendingData_SectoralLendingReports_SectoralLendingReportId",
                table: "SectoralLendingData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SectoralLendingData",
                table: "SectoralLendingData");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "SectoralLendingData");

            migrationBuilder.DropColumn(
                name: "EconomicSectorName",
                table: "SectoralLendingData");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "SectoralLendingData");

            migrationBuilder.RenameTable(
                name: "SectoralLendingData",
                newName: "SectorData");

            migrationBuilder.RenameIndex(
                name: "IX_SectoralLendingData_SectoralLendingReportId",
                table: "SectorData",
                newName: "IX_SectorData_SectoralLendingReportId");

            migrationBuilder.RenameIndex(
                name: "IX_SectoralLendingData_EconomicSectorId",
                table: "SectorData",
                newName: "IX_SectorData_EconomicSectorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SectorData",
                table: "SectorData",
                column: "Id");

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
    }
}
