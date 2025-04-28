using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.Persistence.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class openpracticelapwithdetections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishedUtc",
                table: "OpenPracticeLaps");

            migrationBuilder.DropColumn(
                name: "TotalMilliseconds",
                table: "OpenPracticeLaps");

            migrationBuilder.RenameColumn(
                name: "StartedUtc",
                table: "OpenPracticeLaps",
                newName: "EntryDetectionId");

            migrationBuilder.AddColumn<Guid>(
                name: "ExitDetectionId",
                table: "OpenPracticeLaps",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExitDetectionId",
                table: "OpenPracticeLaps");

            migrationBuilder.RenameColumn(
                name: "EntryDetectionId",
                table: "OpenPracticeLaps",
                newName: "StartedUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedUtc",
                table: "OpenPracticeLaps",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "TotalMilliseconds",
                table: "OpenPracticeLaps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
