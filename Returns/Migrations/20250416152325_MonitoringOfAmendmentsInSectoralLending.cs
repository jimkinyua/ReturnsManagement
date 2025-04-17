using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class MonitoringOfAmendmentsInSectoralLending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "SectorData",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "SectorData",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "SectorData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "SectorData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DaysLateBy",
                table: "SectoralLendingReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "SectoralLendingReports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "SectoralLendingReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAmended",
                table: "SectoralLendingReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "SectoralLendingReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreviousReturnId",
                table: "SectoralLendingReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnId",
                table: "SectoralLendingReports",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "SectoralLendingReports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_SectoralLendingReports_ReturnId",
                table: "SectoralLendingReports",
                column: "ReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_SectoralLendingReports_Returns_ReturnId",
                table: "SectoralLendingReports",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SectoralLendingReports_Returns_ReturnId",
                table: "SectoralLendingReports");

            migrationBuilder.DropIndex(
                name: "IX_SectoralLendingReports_ReturnId",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "SectorData");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "SectorData");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "SectorData");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "SectorData");

            migrationBuilder.DropColumn(
                name: "DaysLateBy",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "IsAmended",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "PreviousReturnId",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "SectoralLendingReports");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "SectoralLendingReports");
        }
    }
}
