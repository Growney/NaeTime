using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventDbLite.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersistedEvents",
                columns: table => new
                {
                    GlobalOrdinal = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StreamName = table.Column<string>(type: "TEXT", nullable: false),
                    StreamOrdinal = table.Column<long>(type: "INTEGER", nullable: false),
                    Metadata = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Payload = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersistedEvents", x => x.GlobalOrdinal);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersistedEvents_StreamName",
                table: "PersistedEvents",
                column: "StreamName");

            migrationBuilder.CreateIndex(
                name: "IX_PersistedEvents_StreamName_StreamOrdinal",
                table: "PersistedEvents",
                columns: new[] { "StreamName", "StreamOrdinal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersistedEvents");
        }
    }
}
