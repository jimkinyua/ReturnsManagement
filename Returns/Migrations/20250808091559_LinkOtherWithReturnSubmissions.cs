using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class LinkOtherWithReturnSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtherReturns_Returns_ReturnId",
                table: "OtherReturns");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "OtherReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "OtherReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OtherReturns_ReturnSubmissionId",
                table: "OtherReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_OtherReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "OtherReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtherReturns_Returns_ReturnId",
                table: "OtherReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtherReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "OtherReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherReturns_Returns_ReturnId",
                table: "OtherReturns");

            migrationBuilder.DropIndex(
                name: "IX_OtherReturns_ReturnSubmissionId",
                table: "OtherReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "OtherReturns");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "OtherReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OtherReturns_Returns_ReturnId",
                table: "OtherReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
