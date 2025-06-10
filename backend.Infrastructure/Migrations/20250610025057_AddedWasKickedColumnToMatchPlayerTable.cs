using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedWasKickedColumnToMatchPlayerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WasKicked",
                table: "MatchPlayers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WasKicked",
                table: "MatchPlayers");
        }
    }
}
