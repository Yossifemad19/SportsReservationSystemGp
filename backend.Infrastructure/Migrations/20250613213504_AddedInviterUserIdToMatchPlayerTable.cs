using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedInviterUserIdToMatchPlayerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvitedByUserId",
                table: "MatchPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayers_InvitedByUserId",
                table: "MatchPlayers",
                column: "InvitedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPlayers_Users_InvitedByUserId",
                table: "MatchPlayers",
                column: "InvitedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchPlayers_Users_InvitedByUserId",
                table: "MatchPlayers");

            migrationBuilder.DropIndex(
                name: "IX_MatchPlayers_InvitedByUserId",
                table: "MatchPlayers");

            migrationBuilder.DropColumn(
                name: "InvitedByUserId",
                table: "MatchPlayers");
        }
    }
}
