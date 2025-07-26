using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class RefactorHowAdditionalIfoIsDon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInformationRequests_Returns_ReturnId",
                table: "AdditionalInformationRequests");

            migrationBuilder.RenameColumn(
                name: "ReturnId",
                table: "AdditionalInformationRequests",
                newName: "ReturnSubmissionId");

            migrationBuilder.RenameIndex(
                name: "IX_AdditionalInformationRequests_ReturnId",
                table: "AdditionalInformationRequests",
                newName: "IX_AdditionalInformationRequests_ReturnSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalInformationRequests_ReturnSubmissions_ReturnSubmissionId",
                table: "AdditionalInformationRequests",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInformationRequests_ReturnSubmissions_ReturnSubmissionId",
                table: "AdditionalInformationRequests");

            migrationBuilder.RenameColumn(
                name: "ReturnSubmissionId",
                table: "AdditionalInformationRequests",
                newName: "ReturnId");

            migrationBuilder.RenameIndex(
                name: "IX_AdditionalInformationRequests_ReturnSubmissionId",
                table: "AdditionalInformationRequests",
                newName: "IX_AdditionalInformationRequests_ReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalInformationRequests_Returns_ReturnId",
                table: "AdditionalInformationRequests",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
