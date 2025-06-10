using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMatchSportId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "SportType",
                table: "Matches");

            migrationBuilder.AddColumn<int>(
                name: "SportId",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_SportId",
                table: "Matches",
                column: "SportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Sports_SportId",
                table: "Matches",
                column: "SportId",
                principalTable: "Sports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Sports_SportId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_SportId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "SportId",
                table: "Matches");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Matches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SportType",
                table: "Matches",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
