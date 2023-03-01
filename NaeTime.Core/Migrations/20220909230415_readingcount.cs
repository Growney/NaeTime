using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.Core.Migrations
{
    public partial class readingcount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeanRssiValue",
                table: "RssiReadingBatches");

            migrationBuilder.AddColumn<int>(
                name: "ReadingCount",
                table: "RssiReadingBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_TrackId",
                table: "Flights",
                column: "TrackId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Tracks_TrackId",
                table: "Flights",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Tracks_TrackId",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_Flights_TrackId",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ReadingCount",
                table: "RssiReadingBatches");

            migrationBuilder.AddColumn<double>(
                name: "MeanRssiValue",
                table: "RssiReadingBatches",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
