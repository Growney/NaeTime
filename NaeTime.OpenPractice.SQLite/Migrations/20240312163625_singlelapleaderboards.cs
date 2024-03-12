using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.OpenPractice.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class singlelapleaderboards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SingleLapLeaderboardPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    CompletionUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LapId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleLapLeaderboardPositions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SingleLapLeaderboardPositions");
        }
    }
}
