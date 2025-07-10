using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class MissingLinkageWithReturnSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyLiquidityReturns_Returns_ReturnId",
                table: "DailyLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_InsiderLendingHeaders_Returns_ReturnId",
                table: "InsiderLendingHeaders");

            migrationBuilder.RenameColumn(
                name: "ReturnId",
                table: "SectoralLendingData",
                newName: "ReturnSubmissionId");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "SectoralLendingReports",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "ManagementReturns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "InsiderLoans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "InsiderLendingHeaders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "InsiderLendingHeaders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DailyLiquidityReturns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "DailyLiquidityReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SectoralLendingReports_ReturnSubmissionId",
                table: "SectoralLendingReports",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_InsiderLendingHeaders_ReturnSubmissionId",
                table: "InsiderLendingHeaders",
                column: "ReturnSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyLiquidityReturns_ReturnSubmissionId",
                table: "DailyLiquidityReturns",
                column: "ReturnSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyLiquidityReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DailyLiquidityReturns",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyLiquidityReturns_Returns_ReturnId",
                table: "DailyLiquidityReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InsiderLendingHeaders_ReturnSubmissions_ReturnSubmissionId",
                table: "InsiderLendingHeaders",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InsiderLendingHeaders_Returns_ReturnId",
                table: "InsiderLendingHeaders",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SectoralLendingReports_ReturnSubmissions_ReturnSubmissionId",
                table: "SectoralLendingReports",
                column: "ReturnSubmissionId",
                principalTable: "ReturnSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyLiquidityReturns_ReturnSubmissions_ReturnSubmissionId",
                table: "DailyLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyLiquidityReturns_Returns_ReturnId",
                table: "DailyLiquidityReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_InsiderLendingHeaders_ReturnSubmissions_ReturnSubmissionId",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_InsiderLendingHeaders_Returns_ReturnId",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropForeignKey(
                name: "FK_SectoralLendingReports_ReturnSubmissions_ReturnSubmissionId",
                table: "SectoralLendingReports");

            migrationBuilder.DropIndex(
                name: "IX_SectoralLendingReports_ReturnSubmissionId",
                table: "SectoralLendingReports");

            migrationBuilder.DropIndex(
                name: "IX_InsiderLendingHeaders_ReturnSubmissionId",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropIndex(
                name: "IX_DailyLiquidityReturns_ReturnSubmissionId",
                table: "DailyLiquidityReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "ManagementReturns");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "InsiderLoans");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "InsiderLendingHeaders");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "DailyLiquidityReturns");

            migrationBuilder.RenameColumn(
                name: "ReturnSubmissionId",
                table: "SectoralLendingData",
                newName: "ReturnId");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "InsiderLendingHeaders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "DailyLiquidityReturns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyLiquidityReturns_Returns_ReturnId",
                table: "DailyLiquidityReturns",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InsiderLendingHeaders_Returns_ReturnId",
                table: "InsiderLendingHeaders",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
