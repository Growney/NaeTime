using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.OpenPractice.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "OpenPracticeLaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinishedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalMilliseconds = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenPracticeLaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenPracticeSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MinimumLapMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    MaximumLapMilliseconds = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenPracticeSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncludedLap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConsecutiveLapLeaderboardPositionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LapId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncludedLap", x => new { x.ConsecutiveLapLeaderboardPositionId, x.Id });
                    table.ForeignKey(
                        name: "FK_IncludedLap_ConsecutiveLapLeaderboardPositions_ConsecutiveLapLeaderboardPositionId",
                        column: x => x.ConsecutiveLapLeaderboardPositionId,
                        principalTable: "ConsecutiveLapLeaderboardPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PilotLane",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpenPracticeSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Lane = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PilotLane", x => new { x.OpenPracticeSessionId, x.Id });
                    table.ForeignKey(
                        name: "FK_PilotLane_OpenPracticeSessions_OpenPracticeSessionId",
                        column: x => x.OpenPracticeSessionId,
                        principalTable: "OpenPracticeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackedConsecutiveLaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OpenPracticeSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LapCap = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedConsecutiveLaps", x => new { x.OpenPracticeSessionId, x.Id });
                    table.ForeignKey(
                        name: "FK_TrackedConsecutiveLaps_OpenPracticeSessions_OpenPracticeSessionId",
                        column: x => x.OpenPracticeSessionId,
                        principalTable: "OpenPracticeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncludedLap");

            migrationBuilder.DropTable(
                name: "OpenPracticeLaps");

            migrationBuilder.DropTable(
                name: "PilotLane");

            migrationBuilder.DropTable(
                name: "TrackedConsecutiveLaps");

            migrationBuilder.DropTable(
                name: "ConsecutiveLapLeaderboardPositions");

            migrationBuilder.DropTable(
                name: "OpenPracticeSessions");
        }
    }
}
