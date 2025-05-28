using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddFormResubmissionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormResubmissionRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FormId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaccoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaccoType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedByEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedByName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChildId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RespondedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResubmissionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormResubmissionRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormResubmissionRequests");
        }
    }
}
