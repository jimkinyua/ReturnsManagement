using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class TrackNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaccoReminderLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SaccoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastReminderSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemindersSentCount = table.Column<int>(type: "int", nullable: false),
                    ReminderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastReminderStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PendingReturns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextReminderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalationLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreferredReminderFrequencyDays = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaccoReminderLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaccoReminderLogs");
        }
    }
}
