using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.Core.Migrations
{
    public partial class flyingsession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FlyingSessionId",
                table: "Flights",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FlyingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HostPilotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlyingSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AllowedPilot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PilotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlyingSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedPilot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedPilot_FlyingSessions_FlyingSessionId",
                        column: x => x.FlyingSessionId,
                        principalTable: "FlyingSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlyingSessionId",
                table: "Flights",
                column: "FlyingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedPilot_FlyingSessionId",
                table: "AllowedPilot",
                column: "FlyingSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_FlyingSessions_FlyingSessionId",
                table: "Flights",
                column: "FlyingSessionId",
                principalTable: "FlyingSessions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_FlyingSessions_FlyingSessionId",
                table: "Flights");

            migrationBuilder.DropTable(
                name: "AllowedPilot");

            migrationBuilder.DropTable(
                name: "FlyingSessions");

            migrationBuilder.DropIndex(
                name: "IX_Flights_FlyingSessionId",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "FlyingSessionId",
                table: "Flights");
        }
    }
}
