using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class maketeamcolumninmatchplayeroptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Team",
                table: "MatchPlayers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRatings_RatedUserId",
                table: "PlayerRatings",
                column: "RatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRatings_RaterUserId",
                table: "PlayerRatings",
                column: "RaterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerRatings_Users_RatedUserId",
                table: "PlayerRatings",
                column: "RatedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerRatings_Users_RaterUserId",
                table: "PlayerRatings",
                column: "RaterUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerRatings_Users_RatedUserId",
                table: "PlayerRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerRatings_Users_RaterUserId",
                table: "PlayerRatings");

            migrationBuilder.DropIndex(
                name: "IX_PlayerRatings_RatedUserId",
                table: "PlayerRatings");

            migrationBuilder.DropIndex(
                name: "IX_PlayerRatings_RaterUserId",
                table: "PlayerRatings");

            migrationBuilder.AlterColumn<string>(
                name: "Team",
                table: "MatchPlayers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
