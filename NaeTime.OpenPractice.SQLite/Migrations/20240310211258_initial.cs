﻿using System;
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
                name: "ConsecutiveLapRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LapCap = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalLaps = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    LastLapCompletionUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsecutiveLapRecords", x => x.Id);
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
                    LapId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConsecutiveLapRecordId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncludedLap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncludedLap_ConsecutiveLapRecords_ConsecutiveLapRecordId",
                        column: x => x.ConsecutiveLapRecordId,
                        principalTable: "ConsecutiveLapRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PilotLane",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Lane = table.Column<byte>(type: "INTEGER", nullable: false),
                    OpenPracticeSessionId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PilotLane", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PilotLane_OpenPracticeSessions_OpenPracticeSessionId",
                        column: x => x.OpenPracticeSessionId,
                        principalTable: "OpenPracticeSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TrackedConsecutiveLaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LapCap = table.Column<uint>(type: "INTEGER", nullable: false),
                    OpenPracticeSessionId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedConsecutiveLaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackedConsecutiveLaps_OpenPracticeSessions_OpenPracticeSessionId",
                        column: x => x.OpenPracticeSessionId,
                        principalTable: "OpenPracticeSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncludedLap_ConsecutiveLapRecordId",
                table: "IncludedLap",
                column: "ConsecutiveLapRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PilotLane_OpenPracticeSessionId",
                table: "PilotLane",
                column: "OpenPracticeSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedConsecutiveLaps_OpenPracticeSessionId",
                table: "TrackedConsecutiveLaps",
                column: "OpenPracticeSessionId");
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
                name: "ConsecutiveLapRecords");

            migrationBuilder.DropTable(
                name: "OpenPracticeSessions");
        }
    }
}
