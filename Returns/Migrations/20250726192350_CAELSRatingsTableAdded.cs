using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class CAELSRatingsTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CAELSRatings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SaccoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RatingDefinitionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CapitalRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AssetQualityRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarningsRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LiquidityRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StructureRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ManagementRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverallRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CalculationDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAELSRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CAELSRatings_RatingDefinations_RatingDefinitionId",
                        column: x => x.RatingDefinitionId,
                        principalTable: "RatingDefinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CAELSRatings_ReturnPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "ReturnPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CAELSRatings_PeriodId",
                table: "CAELSRatings",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_CAELSRatings_RatingDefinitionId",
                table: "CAELSRatings",
                column: "RatingDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CAELSRatings");
        }
    }
}
