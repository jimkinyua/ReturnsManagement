using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCouplingWithEconomicSectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SectoralLendingData_EconomicSectors_EconomicSectorId",
                table: "SectoralLendingData");

            migrationBuilder.AlterColumn<string>(
                name: "EconomicSectorId",
                table: "SectoralLendingData",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "EconomicSectorCode",
                table: "SectoralLendingData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_SectoralLendingData_EconomicSectors_EconomicSectorId",
                table: "SectoralLendingData",
                column: "EconomicSectorId",
                principalTable: "EconomicSectors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SectoralLendingData_EconomicSectors_EconomicSectorId",
                table: "SectoralLendingData");

            migrationBuilder.DropColumn(
                name: "EconomicSectorCode",
                table: "SectoralLendingData");

            migrationBuilder.AlterColumn<string>(
                name: "EconomicSectorId",
                table: "SectoralLendingData",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SectoralLendingData_EconomicSectors_EconomicSectorId",
                table: "SectoralLendingData",
                column: "EconomicSectorId",
                principalTable: "EconomicSectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
