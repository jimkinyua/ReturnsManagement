using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class SomePenindgMigrationsHere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResponded",
                table: "AdditionalInformationRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RequestedBy",
                table: "AdditionalInformationRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "AdditionalInformationRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResponded",
                table: "AdditionalInformationRequests");

            migrationBuilder.DropColumn(
                name: "RequestedBy",
                table: "AdditionalInformationRequests");

            migrationBuilder.DropColumn(
                name: "RespondedAt",
                table: "AdditionalInformationRequests");
        }
    }
}
