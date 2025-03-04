using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.Persistence.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveSession", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActiveTimings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Lane = table.Column<byte>(type: "INTEGER", nullable: false),
                    LapNumber = table.Column<uint>(type: "INTEGER", nullable: false),
                    ActiveLap_Id = table.Column<Guid>(type: "TEXT", nullable: true),
                    ActiveLap_StartedSoftwareTime = table.Column<long>(type: "INTEGER", nullable: true),
                    ActiveLap_StartedUtcTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActiveLap_StartedHardwareTime = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ActiveSplit_Id = table.Column<Guid>(type: "TEXT", nullable: true),
                    ActiveSplit_SplitNumber = table.Column<byte>(type: "INTEGER", nullable: true),
                    ActiveSplit_StartedSoftwareTime = table.Column<long>(type: "INTEGER", nullable: true),
                    ActiveSplit_StartedUtcTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveTimings", x => x.Id);
                });

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
                name: "Detections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Lane = table.Column<byte>(type: "INTEGER", nullable: false),
                    HardwareTime = table.Column<ulong>(type: "INTEGER", nullable: true),
                    SoftwareTime = table.Column<long>(type: "INTEGER", nullable: false),
                    UtcTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsManual = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EthernetLapRF8Channels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EthernetLapRF8Channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lanes",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "INTEGER", nullable: false),
                    BandId = table.Column<byte>(type: "INTEGER", nullable: true),
                    FrequencyInMhz = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lanes", x => x.Id);
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
                name: "Pilots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    CallSign = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pilots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerialEsp32Nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialEsp32Nodes", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "TimerStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConnectionStatusChanged = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WasConnected = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimerStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TotalLapsLeaderboardPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalLaps = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstLapCompletionUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TotalLapsLeaderboardPositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MinimumLapMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    MaximumLapMilliseconds = table.Column<long>(type: "INTEGER", nullable: true),
                    AllowedLanes = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "TrackTimer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimerId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTimer", x => new { x.TrackId, x.Id });
                    table.ForeignKey(
                        name: "FK_TrackTimer_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveSession");

            migrationBuilder.DropTable(
                name: "ActiveTimings");

            migrationBuilder.DropTable(
                name: "AverageLapLeaderboardPositions");

            migrationBuilder.DropTable(
                name: "Detections");

            migrationBuilder.DropTable(
                name: "EthernetLapRF8Channels");

            migrationBuilder.DropTable(
                name: "IncludedLap");

            migrationBuilder.DropTable(
                name: "Lanes");

            migrationBuilder.DropTable(
                name: "OpenPracticeLaps");

            migrationBuilder.DropTable(
                name: "PilotLane");

            migrationBuilder.DropTable(
                name: "Pilots");

            migrationBuilder.DropTable(
                name: "SerialEsp32Nodes");

            migrationBuilder.DropTable(
                name: "SingleLapLeaderboardPositions");

            migrationBuilder.DropTable(
                name: "TimerStatuses");

            migrationBuilder.DropTable(
                name: "TotalLapsLeaderboardPositions");

            migrationBuilder.DropTable(
                name: "TrackedConsecutiveLaps");

            migrationBuilder.DropTable(
                name: "TrackTimer");

            migrationBuilder.DropTable(
                name: "ConsecutiveLapLeaderboardPositions");

            migrationBuilder.DropTable(
                name: "OpenPracticeSessions");

            migrationBuilder.DropTable(
                name: "Tracks");
        }
    }
}
