using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalForm2AFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RetainedEarningsAndDisclosedReservesToCoreCapital",
                table: "NWDTCapitalAdequacyReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAssetValueOffBalanceSheet",
                table: "NWDTCapitalAdequacyReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDepositsLiabilitiesAsPerBalanceSheet",
                table: "NWDTCapitalAdequacyReturns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetainedEarningsAndDisclosedReservesToCoreCapital",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "TotalAssetValueOffBalanceSheet",
                table: "NWDTCapitalAdequacyReturns");

            migrationBuilder.DropColumn(
                name: "TotalDepositsLiabilitiesAsPerBalanceSheet",
                table: "NWDTCapitalAdequacyReturns");
        }
    }
}
