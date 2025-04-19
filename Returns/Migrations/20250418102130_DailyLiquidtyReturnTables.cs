using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class DailyLiquidtyReturnTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyLiquidityReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SACCOName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CSNO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankBalancesOpening = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConsolidatedTreasuryCashBalancesOpening = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TellersBalancesOpening = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MobileMoneyChannelsOpening = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlacementWithBanksOpening = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotalOpening = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositsFromMembers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashLoanRepayments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherCashReceipts = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotalReceipts = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOpeningAndReceipts = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashWithdrawalsByMembers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashPaymentsToMembers = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherCashPayments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotalPayments = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BankBalancesClosing = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConsolidatedTreasuryCashBalancesClosing = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TellersBalancesClosing = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MobileMoneyChannelsClosing = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlacementWithBanksClosing = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BOSADeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FOSADeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalClosingBalanceToTotalDepositsRatio = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalClosingBalanceToFOSADepositsRatio = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyLiquidityReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyLiquidityReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyLiquidityReturns_ReturnId",
                table: "DailyLiquidityReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyLiquidityReturns");
        }
    }
}
