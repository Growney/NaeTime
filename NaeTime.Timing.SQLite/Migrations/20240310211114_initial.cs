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
                    LapNumber = table.Column<uint>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ActiveLap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActiveTimingsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedSoftwareTime = table.Column<long>(type: "INTEGER", nullable: false),
                    StartedUtcTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedHardwareTime = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveLap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveLap_ActiveTimings_ActiveTimingsId",
                        column: x => x.ActiveTimingsId,
                        principalTable: "ActiveTimings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveSplit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActiveTimingsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SplitNumber = table.Column<byte>(type: "INTEGER", nullable: false),
                    StartedSoftwareTime = table.Column<long>(type: "INTEGER", nullable: false),
                    StartedUtcTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveSplit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveSplit_ActiveTimings_ActiveTimingsId",
                        column: x => x.ActiveTimingsId,
                        principalTable: "ActiveTimings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveLap_ActiveTimingsId",
                table: "ActiveLap",
                column: "ActiveTimingsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActiveSplit_ActiveTimingsId",
                table: "ActiveSplit",
                column: "ActiveTimingsId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveLap");

            migrationBuilder.DropTable(
                name: "ActiveSplit");

            migrationBuilder.DropTable(
                name: "Lanes");

            migrationBuilder.DropTable(
                name: "ActiveTimings");
        }
    }
}
