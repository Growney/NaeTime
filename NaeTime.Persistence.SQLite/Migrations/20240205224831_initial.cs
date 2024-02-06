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
                    PilotId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RadioFrequencyInMhz = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lanes", x => x.Id);
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
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MinimumLapMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    MaximumLapMilliseconds = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackTimer",
                columns: table => new
                {
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
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
                name: "Detections");

            migrationBuilder.DropTable(
                name: "EthernetLapRF8Channels");

            migrationBuilder.DropTable(
                name: "Lanes");

            migrationBuilder.DropTable(
                name: "Pilots");

            migrationBuilder.DropTable(
                name: "TimerStatuses");

            migrationBuilder.DropTable(
                name: "TrackTimer");

            migrationBuilder.DropTable(
                name: "Tracks");
        }
    }
}
