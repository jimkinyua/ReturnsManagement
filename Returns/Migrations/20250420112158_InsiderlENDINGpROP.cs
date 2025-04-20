using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class InsiderlENDINGpROP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsiderLendingHeaders_Returns_ReturnsId",
                table: "InsiderLendingHeaders");

            migrationBuilder.RenameColumn(
                name: "ReturnsId",
                table: "InsiderLendingHeaders",
                newName: "ReturnId");

            migrationBuilder.RenameIndex(
                name: "IX_InsiderLendingHeaders_ReturnsId",
                table: "InsiderLendingHeaders",
                newName: "IX_InsiderLendingHeaders_ReturnId");

            migrationBuilder.AddColumn<bool>(
                name: "IsInsiderLending",
                table: "ReturnForms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "InsiderLoans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_InsiderLendingHeaders_Returns_ReturnId",
                table: "InsiderLendingHeaders",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsiderLendingHeaders_Returns_ReturnId",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropColumn(
                name: "IsInsiderLending",
                table: "ReturnForms");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "InsiderLoans");

            migrationBuilder.RenameColumn(
                name: "ReturnId",
                table: "InsiderLendingHeaders",
                newName: "ReturnsId");

            migrationBuilder.RenameIndex(
                name: "IX_InsiderLendingHeaders_ReturnId",
                table: "InsiderLendingHeaders",
                newName: "IX_InsiderLendingHeaders_ReturnsId");

            migrationBuilder.AddForeignKey(
                name: "FK_InsiderLendingHeaders_Returns_ReturnsId",
                table: "InsiderLendingHeaders",
                column: "ReturnsId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
