using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class CamelRatingSetUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CamelCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CamelCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CamelIndicators",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BetterHigher = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CamelIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CamelIndicators_CamelCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CamelCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndicatorRatingThresholds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IndicatorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RatingLevel = table.Column<int>(type: "int", nullable: false),
                    ThresholdValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorRatingThresholds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicatorRatingThresholds_CamelIndicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalTable: "CamelIndicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CamelIndicators_CategoryId",
                table: "CamelIndicators",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorRatingThresholds_IndicatorId",
                table: "IndicatorRatingThresholds",
                column: "IndicatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndicatorRatingThresholds");

            migrationBuilder.DropTable(
                name: "CamelIndicators");

            migrationBuilder.DropTable(
                name: "CamelCategories");
        }
    }
}
