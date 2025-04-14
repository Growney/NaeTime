using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.Persistence.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class lanekeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TimerLaneConfigurations",
                table: "TimerLaneConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LapRFConfigurations",
                table: "LapRFConfigurations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimerLaneConfigurations",
                table: "TimerLaneConfigurations",
                columns: new[] { "Id", "LaneId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_LapRFConfigurations",
                table: "LapRFConfigurations",
                columns: new[] { "Id", "Lane" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TimerLaneConfigurations",
                table: "TimerLaneConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LapRFConfigurations",
                table: "LapRFConfigurations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimerLaneConfigurations",
                table: "TimerLaneConfigurations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LapRFConfigurations",
                table: "LapRFConfigurations",
                column: "Id");
        }
    }
}
