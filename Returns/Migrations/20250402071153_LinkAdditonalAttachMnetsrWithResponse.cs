using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class LinkAdditonalAttachMnetsrWithResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInfoReponse_AdditionalInformationRequest_AdditionalInformationRequestId",
                table: "AdditionalInfoReponse");

            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInformationRequest_Returns_ReturnId",
                table: "AdditionalInformationRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_ResponseAttachement_AdditionalInfoReponse_AdditionalInfoReponseId",
                table: "ResponseAttachement");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnsAssigment_Returns_ReturnId",
                table: "ReturnsAssigment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReturnsAssigment",
                table: "ReturnsAssigment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResponseAttachement",
                table: "ResponseAttachement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdditionalInformationRequest",
                table: "AdditionalInformationRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdditionalInfoReponse",
                table: "AdditionalInfoReponse");

            migrationBuilder.RenameTable(
                name: "ReturnsAssigment",
                newName: "ReturnsAssigments");

            migrationBuilder.RenameTable(
                name: "ResponseAttachement",
                newName: "ResponseAttachements");

            migrationBuilder.RenameTable(
                name: "AdditionalInformationRequest",
                newName: "AdditionalInformationRequests");

            migrationBuilder.RenameTable(
                name: "AdditionalInfoReponse",
                newName: "AdditionalInfoReponses");

            migrationBuilder.RenameIndex(
                name: "IX_ReturnsAssigment_ReturnId",
                table: "ReturnsAssigments",
                newName: "IX_ReturnsAssigments_ReturnId");

            migrationBuilder.RenameIndex(
                name: "IX_ResponseAttachement_AdditionalInfoReponseId",
                table: "ResponseAttachements",
                newName: "IX_ResponseAttachements_AdditionalInfoReponseId");

            migrationBuilder.RenameIndex(
                name: "IX_AdditionalInformationRequest_ReturnId",
                table: "AdditionalInformationRequests",
                newName: "IX_AdditionalInformationRequests_ReturnId");

            migrationBuilder.RenameIndex(
                name: "IX_AdditionalInfoReponse_AdditionalInformationRequestId",
                table: "AdditionalInfoReponses",
                newName: "IX_AdditionalInfoReponses_AdditionalInformationRequestId");

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalInformationRequestId",
                table: "AdditionalInfoReponses",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReturnsAssigments",
                table: "ReturnsAssigments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResponseAttachements",
                table: "ResponseAttachements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdditionalInformationRequests",
                table: "AdditionalInformationRequests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdditionalInfoReponses",
                table: "AdditionalInfoReponses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalInfoReponses_AdditionalInformationRequests_AdditionalInformationRequestId",
                table: "AdditionalInfoReponses",
                column: "AdditionalInformationRequestId",
                principalTable: "AdditionalInformationRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalInformationRequests_Returns_ReturnId",
                table: "AdditionalInformationRequests",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseAttachements_AdditionalInfoReponses_AdditionalInfoReponseId",
                table: "ResponseAttachements",
                column: "AdditionalInfoReponseId",
                principalTable: "AdditionalInfoReponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnsAssigments_Returns_ReturnId",
                table: "ReturnsAssigments",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInfoReponses_AdditionalInformationRequests_AdditionalInformationRequestId",
                table: "AdditionalInfoReponses");

            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalInformationRequests_Returns_ReturnId",
                table: "AdditionalInformationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ResponseAttachements_AdditionalInfoReponses_AdditionalInfoReponseId",
                table: "ResponseAttachements");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnsAssigments_Returns_ReturnId",
                table: "ReturnsAssigments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReturnsAssigments",
                table: "ReturnsAssigments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResponseAttachements",
                table: "ResponseAttachements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdditionalInformationRequests",
                table: "AdditionalInformationRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdditionalInfoReponses",
                table: "AdditionalInfoReponses");

            migrationBuilder.RenameTable(
                name: "ReturnsAssigments",
                newName: "ReturnsAssigment");

            migrationBuilder.RenameTable(
                name: "ResponseAttachements",
                newName: "ResponseAttachement");

            migrationBuilder.RenameTable(
                name: "AdditionalInformationRequests",
                newName: "AdditionalInformationRequest");

            migrationBuilder.RenameTable(
                name: "AdditionalInfoReponses",
                newName: "AdditionalInfoReponse");

            migrationBuilder.RenameIndex(
                name: "IX_ReturnsAssigments_ReturnId",
                table: "ReturnsAssigment",
                newName: "IX_ReturnsAssigment_ReturnId");

            migrationBuilder.RenameIndex(
                name: "IX_ResponseAttachements_AdditionalInfoReponseId",
                table: "ResponseAttachement",
                newName: "IX_ResponseAttachement_AdditionalInfoReponseId");

            migrationBuilder.RenameIndex(
                name: "IX_AdditionalInformationRequests_ReturnId",
                table: "AdditionalInformationRequest",
                newName: "IX_AdditionalInformationRequest_ReturnId");

            migrationBuilder.RenameIndex(
                name: "IX_AdditionalInfoReponses_AdditionalInformationRequestId",
                table: "AdditionalInfoReponse",
                newName: "IX_AdditionalInfoReponse_AdditionalInformationRequestId");

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalInformationRequestId",
                table: "AdditionalInfoReponse",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReturnsAssigment",
                table: "ReturnsAssigment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResponseAttachement",
                table: "ResponseAttachement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdditionalInformationRequest",
                table: "AdditionalInformationRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdditionalInfoReponse",
                table: "AdditionalInfoReponse",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalInfoReponse_AdditionalInformationRequest_AdditionalInformationRequestId",
                table: "AdditionalInfoReponse",
                column: "AdditionalInformationRequestId",
                principalTable: "AdditionalInformationRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalInformationRequest_Returns_ReturnId",
                table: "AdditionalInformationRequest",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseAttachement_AdditionalInfoReponse_AdditionalInfoReponseId",
                table: "ResponseAttachement",
                column: "AdditionalInfoReponseId",
                principalTable: "AdditionalInfoReponse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnsAssigment_Returns_ReturnId",
                table: "ReturnsAssigment",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
