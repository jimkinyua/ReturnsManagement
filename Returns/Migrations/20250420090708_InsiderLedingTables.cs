using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class InsiderLedingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsiderLendingHeaders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SaccoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaccoName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CSNO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysLateBy = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsiderLendingHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsiderLendingHeaders_Returns_ReturnsId",
                        column: x => x.ReturnsId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InsiderLoans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameOfBorrower = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionHeld = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountAppliedFor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountGranted = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateApprovedOrRatified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountOfBosaDeposits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NatureOfSecurity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepaymentCommencementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepaymentPeriod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OtherRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PerfomanceCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InsiderLendingHeaderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAmended = table.Column<bool>(type: "bit", nullable: false),
                    PreviousReturnId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsiderLoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsiderLoans_InsiderLendingHeaders_InsiderLendingHeaderId",
                        column: x => x.InsiderLendingHeaderId,
                        principalTable: "InsiderLendingHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsiderLendingHeaders_ReturnsId",
                table: "InsiderLendingHeaders",
                column: "ReturnsId");

            migrationBuilder.CreateIndex(
                name: "IX_InsiderLoans_InsiderLendingHeaderId",
                table: "InsiderLoans",
                column: "InsiderLendingHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsiderLoans");

            migrationBuilder.DropTable(
                name: "InsiderLendingHeaders");
        }
    }
}
