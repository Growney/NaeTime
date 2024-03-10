using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.OpenPractice.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class consecutiveleaderboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncludedLap");

            migrationBuilder.CreateTable(
                name: "ConsecutiveLapLeaderboardPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LapCap = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalLaps = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    LastLapCompletionUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsecutiveLapLeaderboardPositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsecutiveLapRecords_IncludedLaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConsecutiveLapRecordId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LapId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsecutiveLapRecords_IncludedLaps", x => new { x.ConsecutiveLapRecordId, x.Id });
                    table.ForeignKey(
                        name: "FK_ConsecutiveLapRecords_IncludedLaps_ConsecutiveLapRecords_ConsecutiveLapRecordId",
                        column: x => x.ConsecutiveLapRecordId,
                        principalTable: "ConsecutiveLapRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsecutiveLapLeaderboardPositions_IncludedLaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConsecutiveLapLeaderboardPositionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LapId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsecutiveLapLeaderboardPositions_IncludedLaps", x => new { x.ConsecutiveLapLeaderboardPositionId, x.Id });
                    table.ForeignKey(
                        name: "FK_ConsecutiveLapLeaderboardPositions_IncludedLaps_ConsecutiveLapLeaderboardPositions_ConsecutiveLapLeaderboardPositionId",
                        column: x => x.ConsecutiveLapLeaderboardPositionId,
                        principalTable: "ConsecutiveLapLeaderboardPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsecutiveLapLeaderboardPositions_IncludedLaps");

            migrationBuilder.DropTable(
                name: "ConsecutiveLapRecords_IncludedLaps");

            migrationBuilder.DropTable(
                name: "ConsecutiveLapLeaderboardPositions");

            migrationBuilder.CreateTable(
                name: "IncludedLap",
                columns: table => new
                {
                    ConsecutiveLapRecordId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LapId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncludedLap", x => new { x.ConsecutiveLapRecordId, x.Id });
                    table.ForeignKey(
                        name: "FK_IncludedLap_ConsecutiveLapRecords_ConsecutiveLapRecordId",
                        column: x => x.ConsecutiveLapRecordId,
                        principalTable: "ConsecutiveLapRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
