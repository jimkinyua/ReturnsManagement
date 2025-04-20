using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSaccoID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaccoId",
                table: "InsiderLendingHeaders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SaccoId",
                table: "InsiderLendingHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
