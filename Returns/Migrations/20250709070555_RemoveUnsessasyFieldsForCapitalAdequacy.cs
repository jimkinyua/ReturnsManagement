using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnsessasyFieldsForCapitalAdequacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "DTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "DTCapitalAdequacyReturns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "DTCapitalAdequacyReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "DTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "DTCapitalAdequacyReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
