using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixedMatchPlayerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchPlayers_Users_InvitedByUserId",
                table: "MatchPlayers");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPlayers_Users_InvitedByUserId",
                table: "MatchPlayers",
                column: "InvitedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchPlayers_Users_InvitedByUserId",
                table: "MatchPlayers");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPlayers_Users_InvitedByUserId",
                table: "MatchPlayers",
                column: "InvitedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
