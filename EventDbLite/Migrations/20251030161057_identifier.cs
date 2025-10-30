using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventDbLite.Migrations
{
    /// <inheritdoc />
    public partial class identifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Identifier",
                table: "PersistedEvents",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "PersistedEvents");
        }
    }
}
