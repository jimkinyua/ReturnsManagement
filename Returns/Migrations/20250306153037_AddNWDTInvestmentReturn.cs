using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddNWDTInvestmentReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NWDTInvestmentReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonEarningAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubsidiaryRelatedEntityInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EquityInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherInvestments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAssetsLandBuildingEquipment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandAndBuilding = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxLandBuildingEquipmentToTotalAssetRequirement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxLandBuildingToTotalAssetRequirement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxFinancialInvestmentsToCoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxEquityInvestmentsToTotalDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxSubsidiaryInvestmentToTotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxOtherInvestmentsToCoreCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandBuildingEquipmentToTotalAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandBuildingEquipmentExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandBuildingToTotalAssetsRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandBuildingExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestmentsToCoreCapitalRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialInvestmentsExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EquityInvestmentsToCoreCapitalRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EquityInvestmentsExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubsidiaryInvestmentsToCoreCapitalRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubsidiaryInvestmentsExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherInvestmentsToCoreCapitalRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherInvestmentsExcessDeficiency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NWDTInvestmentReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NWDTInvestmentReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NWDTInvestmentReturns_ReturnId",
                table: "NWDTInvestmentReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NWDTInvestmentReturns");
        }
    }
}
