using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.Timing.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveTimings");

            migrationBuilder.DropTable(
                name: "Lanes");
        }
    }
}
