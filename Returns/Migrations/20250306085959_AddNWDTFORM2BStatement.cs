using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddNWDTFORM2BStatement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NDWTLiquidityReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocalNotesAndCoins = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ForeignNotesAndCoins = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithCommercialBanks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TimeDepositsWithBanksMoreThan90Days = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverdraftsAndMaturedLoans = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithOtherSaccoSocieties = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesWithOtherFinancialInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToOtherSaccoSocieties = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalancesDueToFinancialInstitutions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaturedLoansAndAdvances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TreasuryBills = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TreasuryBondsBearerBonds = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaturedLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LiabilitiesMaturing91Days = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumRequirement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NDWTLiquidityReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NDWTLiquidityReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NDWTLiquidityReturns_ReturnId",
                table: "NDWTLiquidityReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NDWTLiquidityReturns");
        }
    }
}
