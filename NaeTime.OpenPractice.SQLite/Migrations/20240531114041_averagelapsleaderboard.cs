using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.OpenPractice.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class averagelapsleaderboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AverageLapLeaderboardPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AverageMilliseconds = table.Column<double>(type: "REAL", nullable: false),
                    FirstLapCompletionUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AverageLapLeaderboardPositions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AverageLapLeaderboardPositions");
        }
    }
}
