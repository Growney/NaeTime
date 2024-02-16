﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.Persistence.SQLite.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
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
            constraints: table => table.PrimaryKey("PK_ActiveSession", x => x.Id));

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
            constraints: table => table.PrimaryKey("PK_ActiveTimings", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ConsecutiveLapLeaderboards",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                ConsecutiveLaps = table.Column<uint>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ConsecutiveLapLeaderboards", x => x.Id));

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
            constraints: table => table.PrimaryKey("PK_Detections", x => x.Id));

        migrationBuilder.CreateTable(
            name: "EthernetLapRF8Channels",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                IpAddress = table.Column<byte[]>(type: "BLOB", nullable: false),
                Port = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_EthernetLapRF8Channels", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Lanes",
            columns: table => new
            {
                Id = table.Column<byte>(type: "INTEGER", nullable: false),
                BandId = table.Column<byte>(type: "INTEGER", nullable: true),
                RadioFrequencyInMhz = table.Column<int>(type: "INTEGER", nullable: false),
                IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Lanes", x => x.Id));

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
            constraints: table => table.PrimaryKey("PK_OpenPracticeLaps", x => x.Id));

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
            constraints: table => table.PrimaryKey("PK_OpenPracticeSessions", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Pilots",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                FirstName = table.Column<string>(type: "TEXT", nullable: true),
                LastName = table.Column<string>(type: "TEXT", nullable: true),
                CallSign = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Pilots", x => x.Id));

        migrationBuilder.CreateTable(
            name: "SingleLapLeaderboards",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                SessionId = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_SingleLapLeaderboards", x => x.Id));

        migrationBuilder.CreateTable(
            name: "TimerStatuses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ConnectionStatusChanged = table.Column<DateTime>(type: "TEXT", nullable: true),
                WasConnected = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_TimerStatuses", x => x.Id));

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
            constraints: table => table.PrimaryKey("PK_Tracks", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ConsecutiveLapLeaderboardPosition",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ConsecutiveLapLeaderboardId = table.Column<Guid>(type: "TEXT", nullable: false),
                Position = table.Column<uint>(type: "INTEGER", nullable: false),
                PilotId = table.Column<Guid>(type: "TEXT", nullable: false),
                TotalLaps = table.Column<uint>(type: "INTEGER", nullable: false),
                TotalMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                LastLapCompletionUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConsecutiveLapLeaderboardPosition", x => new { x.ConsecutiveLapLeaderboardId, x.Id });
                table.ForeignKey(
                    name: "FK_ConsecutiveLapLeaderboardPosition_ConsecutiveLapLeaderboards_ConsecutiveLapLeaderboardId",
                    column: x => x.ConsecutiveLapLeaderboardId,
                    principalTable: "ConsecutiveLapLeaderboards",
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
            name: "SingleLapLeaderboardPosition",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                SingleLapLeaderboardId = table.Column<Guid>(type: "TEXT", nullable: false),
                Position = table.Column<uint>(type: "INTEGER", nullable: false),
                PilotId = table.Column<Guid>(type: "TEXT", nullable: false),
                LapId = table.Column<Guid>(type: "TEXT", nullable: false),
                LapMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                CompletionUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SingleLapLeaderboardPosition", x => new { x.SingleLapLeaderboardId, x.Id });
                table.ForeignKey(
                    name: "FK_SingleLapLeaderboardPosition_SingleLapLeaderboards_SingleLapLeaderboardId",
                    column: x => x.SingleLapLeaderboardId,
                    principalTable: "SingleLapLeaderboards",
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

        migrationBuilder.CreateTable(
            name: "ConsecutiveLapLeaderboardPositionLap",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ConsecutiveLapLeaderboardPositionConsecutiveLapLeaderboardId = table.Column<Guid>(type: "TEXT", nullable: false),
                ConsecutiveLapLeaderboardPositionId = table.Column<Guid>(type: "TEXT", nullable: false),
                Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                LapId = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConsecutiveLapLeaderboardPositionLap", x => new { x.ConsecutiveLapLeaderboardPositionConsecutiveLapLeaderboardId, x.ConsecutiveLapLeaderboardPositionId, x.Id });
                table.ForeignKey(
                    name: "FK_ConsecutiveLapLeaderboardPositionLap_ConsecutiveLapLeaderboardPosition_ConsecutiveLapLeaderboardPositionConsecutiveLapLeaderboardId_ConsecutiveLapLeaderboardPositionId",
                    columns: x => new { x.ConsecutiveLapLeaderboardPositionConsecutiveLapLeaderboardId, x.ConsecutiveLapLeaderboardPositionId },
                    principalTable: "ConsecutiveLapLeaderboardPosition",
                    principalColumns: new[] { "ConsecutiveLapLeaderboardId", "Id" },
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
            name: "ConsecutiveLapLeaderboardPositionLap");

        migrationBuilder.DropTable(
            name: "Detections");

        migrationBuilder.DropTable(
            name: "EthernetLapRF8Channels");

        migrationBuilder.DropTable(
            name: "Lanes");

        migrationBuilder.DropTable(
            name: "OpenPracticeLaps");

        migrationBuilder.DropTable(
            name: "PilotLane");

        migrationBuilder.DropTable(
            name: "Pilots");

        migrationBuilder.DropTable(
            name: "SingleLapLeaderboardPosition");

        migrationBuilder.DropTable(
            name: "TimerStatuses");

        migrationBuilder.DropTable(
            name: "TrackTimer");

        migrationBuilder.DropTable(
            name: "ConsecutiveLapLeaderboardPosition");

        migrationBuilder.DropTable(
            name: "OpenPracticeSessions");

        migrationBuilder.DropTable(
            name: "SingleLapLeaderboards");

        migrationBuilder.DropTable(
            name: "Tracks");

        migrationBuilder.DropTable(
            name: "ConsecutiveLapLeaderboards");
    }
}
