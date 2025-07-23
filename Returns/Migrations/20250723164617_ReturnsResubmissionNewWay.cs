using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class ReturnsResubmissionNewWay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmendedBySubmissionId",
                table: "ReturnSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AmendsSubmissionId",
                table: "ReturnSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLatest",
                table: "ReturnSubmissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "ReturnSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AmendmentRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SaccoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedReturnId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnSubmissionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmendmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmendmentRequests_ReturnSubmissions_ReturnSubmissionId",
                        column: x => x.ReturnSubmissionId,
                        principalTable: "ReturnSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmendmentRequests_ReturnSubmissionId",
                table: "AmendmentRequests",
                column: "ReturnSubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmendmentRequests");

            migrationBuilder.DropColumn(
                name: "AmendedBySubmissionId",
                table: "ReturnSubmissions");

            migrationBuilder.DropColumn(
                name: "AmendsSubmissionId",
                table: "ReturnSubmissions");

            migrationBuilder.DropColumn(
                name: "IsLatest",
                table: "ReturnSubmissions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ReturnSubmissions");
        }
    }
}
