using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddNWDTRiskClassificationReturnTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NWDTRiskClassificationReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfAccounts = table.Column<int>(type: "int", nullable: true),
                    OutstandingLoanPortfolio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequiredProvision = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequiredProvisionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NWDTRiskClassificationReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NWDTRiskClassificationReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NWDTRiskClassificationReturns_ReturnId",
                table: "NWDTRiskClassificationReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NWDTRiskClassificationReturns");
        }
    }
}
