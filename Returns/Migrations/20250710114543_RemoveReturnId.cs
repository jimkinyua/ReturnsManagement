using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReturnId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SectoralLendingReports_Returns_ReturnId",
                table: "SectoralLendingReports");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "SectoralLendingReports",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_SectoralLendingReports_Returns_ReturnId",
                table: "SectoralLendingReports",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SectoralLendingReports_Returns_ReturnId",
                table: "SectoralLendingReports");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "SectoralLendingReports",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SectoralLendingReports_Returns_ReturnId",
                table: "SectoralLendingReports",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
