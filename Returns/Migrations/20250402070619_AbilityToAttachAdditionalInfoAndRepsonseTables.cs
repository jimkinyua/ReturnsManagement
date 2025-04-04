using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AbilityToAttachAdditionalInfoAndRepsonseTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalInformationRequest",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalInformationRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalInformationRequest_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalInfoReponse",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestForInfoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalInformationRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalInfoReponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoReponse_AdditionalInformationRequest_AdditionalInformationRequestId",
                        column: x => x.AdditionalInformationRequestId,
                        principalTable: "AdditionalInformationRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResponseAttachement",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalInfoReponseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseAttachement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponseAttachement_AdditionalInfoReponse_AdditionalInfoReponseId",
                        column: x => x.AdditionalInfoReponseId,
                        principalTable: "AdditionalInfoReponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoReponse_AdditionalInformationRequestId",
                table: "AdditionalInfoReponse",
                column: "AdditionalInformationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInformationRequest_ReturnId",
                table: "AdditionalInformationRequest",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseAttachement_AdditionalInfoReponseId",
                table: "ResponseAttachement",
                column: "AdditionalInfoReponseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResponseAttachement");

            migrationBuilder.DropTable(
                name: "AdditionalInfoReponse");

            migrationBuilder.DropTable(
                name: "AdditionalInformationRequest");
        }
    }
}
