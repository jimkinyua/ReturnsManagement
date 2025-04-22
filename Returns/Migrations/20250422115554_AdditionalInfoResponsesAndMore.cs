using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalInfoResponsesAndMore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResponseAttachements_AdditionalInfoReponses_AdditionalInfoReponseId",
                table: "ResponseAttachements");

            migrationBuilder.DropTable(
                name: "AdditionalInfoReponses");

            migrationBuilder.RenameColumn(
                name: "AdditionalInfoReponseId",
                table: "ResponseAttachements",
                newName: "ResponseId");

            migrationBuilder.RenameIndex(
                name: "IX_ResponseAttachements_AdditionalInfoReponseId",
                table: "ResponseAttachements",
                newName: "IX_ResponseAttachements_ResponseId");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "ResponseAttachements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequestStatus",
                table: "AdditionalInformationRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaccoId",
                table: "AdditionalInformationRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AdditionalInfoResponses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RespondedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalInformationRequestId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalInfoResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoResponses_AdditionalInformationRequests_AdditionalInformationRequestId",
                        column: x => x.AdditionalInformationRequestId,
                        principalTable: "AdditionalInformationRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoResponses_AdditionalInformationRequestId",
                table: "AdditionalInfoResponses",
                column: "AdditionalInformationRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseAttachements_AdditionalInfoResponses_ResponseId",
                table: "ResponseAttachements",
                column: "ResponseId",
                principalTable: "AdditionalInfoResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResponseAttachements_AdditionalInfoResponses_ResponseId",
                table: "ResponseAttachements");

            migrationBuilder.DropTable(
                name: "AdditionalInfoResponses");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "ResponseAttachements");

            migrationBuilder.DropColumn(
                name: "RequestStatus",
                table: "AdditionalInformationRequests");

            migrationBuilder.DropColumn(
                name: "SaccoId",
                table: "AdditionalInformationRequests");

            migrationBuilder.RenameColumn(
                name: "ResponseId",
                table: "ResponseAttachements",
                newName: "AdditionalInfoReponseId");

            migrationBuilder.RenameIndex(
                name: "IX_ResponseAttachements_ResponseId",
                table: "ResponseAttachements",
                newName: "IX_ResponseAttachements_AdditionalInfoReponseId");

            migrationBuilder.CreateTable(
                name: "AdditionalInfoReponses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdditionalInformationRequestId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestForInfoId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalInfoReponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoReponses_AdditionalInformationRequests_AdditionalInformationRequestId",
                        column: x => x.AdditionalInformationRequestId,
                        principalTable: "AdditionalInformationRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoReponses_AdditionalInformationRequestId",
                table: "AdditionalInfoReponses",
                column: "AdditionalInformationRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseAttachements_AdditionalInfoReponses_AdditionalInfoReponseId",
                table: "ResponseAttachements",
                column: "AdditionalInfoReponseId",
                principalTable: "AdditionalInfoReponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
