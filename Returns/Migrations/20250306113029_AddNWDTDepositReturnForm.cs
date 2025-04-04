using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AddNWDTDepositReturnForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NWDTDepositReturns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RangeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepositType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfAccounts = table.Column<int>(type: "int", nullable: false),
                    AmountInKshs000 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NWDTDepositReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NWDTDepositReturns_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NWDTDepositReturns_ReturnId",
                table: "NWDTDepositReturns",
                column: "ReturnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NWDTDepositReturns");
        }
    }
}
