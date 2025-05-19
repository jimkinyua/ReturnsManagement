using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class ManagementToolAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManagementReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GorvenanceStructureScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GorvenanceStructureWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GorvenanceStructureWeightedScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InternalControlsScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InternalControlsWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InternalControlsWeightedScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InternalScoreScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InternalScoreWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InternalScoreWeightedScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComplianceWithLawsAndRegulationsScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComplianceWithLawsAndRegulationsWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComplianceWithLawsAndRegulationsWeightedScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MemberProtectionScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MemberProtectionWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MemberProtectionWeightedScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdequacyOfMISScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdequacyOfMISWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdequacyOfMISWeightedScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverallRiskProfileScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverallRiskProfileWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverallRiskProfileWeightedScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    SaccoCsNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagementReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementReturns_ReturnId",
                table: "ManagementReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagementReturns");
        }
    }
}
