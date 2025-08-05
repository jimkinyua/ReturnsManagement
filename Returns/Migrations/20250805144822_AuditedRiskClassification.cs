using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AuditedRiskClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditedRiskClassifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfAccounts = table.Column<int>(type: "int", nullable: true),
                    OutstandingLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequiredProvision = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequiredProvisionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnSubmissionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SaccoCsNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    FormId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiresResubmission = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditedRiskClassifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditedRiskClassifications_ReturnSubmissions_ReturnSubmissionId",
                        column: x => x.ReturnSubmissionId,
                        principalTable: "ReturnSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditedRiskClassifications_ReturnSubmissionId",
                table: "AuditedRiskClassifications",
                column: "ReturnSubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditedRiskClassifications");
        }
    }
}
