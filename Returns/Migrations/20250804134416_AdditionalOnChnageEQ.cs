using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalOnChnageEQ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SaccoName",
                table: "AmendmentRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoType",
                table: "AmendmentRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaccoName",
                table: "AmendmentRequests");

            migrationBuilder.DropColumn(
                name: "SaccoType",
                table: "AmendmentRequests");
        }
    }
}
