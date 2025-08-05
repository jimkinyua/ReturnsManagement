using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReturnSubmissionId",
                table: "AuditedFinancialPositions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_AuditedFinancialPositions_ReturnSubmissionId",
                table: "AuditedFinancialPositions",
                column: "ReturnSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditedFinancialPositions_ReturnSubmissions_ReturnSubmissionId",
                table: "AuditedFinancialPositions",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditedFinancialPositions_ReturnSubmissions_ReturnSubmissionId",
                table: "AuditedFinancialPositions");

            migrationBuilder.DropIndex(
                name: "IX_AuditedFinancialPositions_ReturnSubmissionId",
                table: "AuditedFinancialPositions");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnSubmissionId",
                table: "AuditedFinancialPositions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
