using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCouplingWithMagtAndReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManagementReturns_Returns_ReturnId",
                table: "ManagementReturns");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnSubmissionId",
                table: "ManagementReturns",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "ManagementReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "FormId",
                table: "ManagementReturns",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementReturns_ReturnSubmissionId",
                table: "ManagementReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ManagementReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "ManagementReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManagementReturns_Returns_ReturnId",
                table: "ManagementReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManagementReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "ManagementReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_ManagementReturns_Returns_ReturnId",
                table: "ManagementReturns");

            migrationBuilder.DropIndex(
                name: "IX_ManagementReturns_ReturnSubmissionId",
                table: "ManagementReturns");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnSubmissionId",
                table: "ManagementReturns",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "ManagementReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FormId",
                table: "ManagementReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ManagementReturns_Returns_ReturnId",
                table: "ManagementReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
