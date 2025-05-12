using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaeTime.Persistence.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class ELRS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BindingPhrase",
                table: "Pilots",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SerialELRSBackpacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialELRSBackpacks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerialELRSBackpacks");

            migrationBuilder.DropColumn(
                name: "BindingPhrase",
                table: "Pilots");
        }
    }
}
