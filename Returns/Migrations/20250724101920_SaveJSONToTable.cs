using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class SaveJSONToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExpectedReturnId",
                table: "AmendmentRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ContentsJson",
                table: "AmendmentRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "AmendmentRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ParseErrorsJson",
                table: "AmendmentRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ParseSuccess",
                table: "AmendmentRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AmendmentRequests_ExpectedReturnId",
                table: "AmendmentRequests",
                column: "ExpectedReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_AmendmentRequests_ExpectedReturns_ExpectedReturnId",
                table: "AmendmentRequests",
                column: "ExpectedReturnId",
                principalTable: "ExpectedReturns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AmendmentRequests_ExpectedReturns_ExpectedReturnId",
                table: "AmendmentRequests");

            migrationBuilder.DropIndex(
                name: "IX_AmendmentRequests_ExpectedReturnId",
                table: "AmendmentRequests");

            migrationBuilder.DropColumn(
                name: "ContentsJson",
                table: "AmendmentRequests");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "AmendmentRequests");

            migrationBuilder.DropColumn(
                name: "ParseErrorsJson",
                table: "AmendmentRequests");

            migrationBuilder.DropColumn(
                name: "ParseSuccess",
                table: "AmendmentRequests");

            migrationBuilder.AlterColumn<string>(
                name: "ExpectedReturnId",
                table: "AmendmentRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
