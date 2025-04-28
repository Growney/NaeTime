using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.Persistence.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class openpracticedetections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedLanes",
                table: "Tracks");

            migrationBuilder.AddColumn<int>(
                name: "OrdinalPosition",
                table: "TrackTimer",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OpenPracticeDetections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimerIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    Lane = table.Column<byte>(type: "INTEGER", nullable: false),
                    PilotId = table.Column<Guid>(type: "TEXT", nullable: true),
                    HardwareTime = table.Column<ulong>(type: "INTEGER", nullable: true),
                    SoftwareTime = table.Column<long>(type: "INTEGER", nullable: false),
                    UtcTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenPracticeDetections", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenPracticeDetections");

            migrationBuilder.DropColumn(
                name: "OrdinalPosition",
                table: "TrackTimer");

            migrationBuilder.AddColumn<byte>(
                name: "AllowedLanes",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
