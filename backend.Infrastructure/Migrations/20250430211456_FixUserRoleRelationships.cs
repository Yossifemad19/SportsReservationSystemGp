using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRoleRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First create the UserRoles table 
            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleName",
                table: "UserRoles",
                column: "RoleName",
                unique: true);

            // Seed the UserRoles
            migrationBuilder.Sql(@"
                INSERT INTO ""UserRoles"" (""RoleName"", ""Description"")
                VALUES ('User', 'Regular user role');
                
                INSERT INTO ""UserRoles"" (""RoleName"", ""Description"")
                VALUES ('Admin', 'Administrator role');
                
                INSERT INTO ""UserRoles"" (""RoleName"", ""Description"")
                VALUES ('Owner', 'Facility owner role');
            ");

            // Now handle the entity modifications
            migrationBuilder.DropColumn(
                name: "UserRole",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "UserRole",
                table: "Admins");

            migrationBuilder.RenameColumn(
                name: "UserRole",
                table: "Users",
                newName: "UserName");

            // Add UserRoleId columns
            migrationBuilder.AddColumn<int>(
                name: "UserRoleId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 1); // Default to 'User' role

            migrationBuilder.AddColumn<int>(
                name: "UserRoleId",
                table: "Owners",
                type: "integer",
                nullable: false,
                defaultValue: 3); // Default to 'Owner' role

            migrationBuilder.AddColumn<int>(
                name: "UserRoleId",
                table: "Admins",
                type: "integer",
                nullable: false,
                defaultValue: 2); // Default to 'Admin' role

            migrationBuilder.CreateTable(
                name: "PlayerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SkillLevel = table.Column<int>(type: "integer", nullable: false),
                    SportSpecificSkillsJson = table.Column<string>(type: "text", nullable: false),
                    PreferredPlayingStyle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreferredSportsJson = table.Column<string>(type: "text", nullable: false),
                    PreferredPlayingTimesJson = table.Column<string>(type: "text", nullable: false),
                    PrefersCompetitivePlay = table.Column<bool>(type: "boolean", nullable: false),
                    PrefersCasualPlay = table.Column<bool>(type: "boolean", nullable: false),
                    PreferredTeamSize = table.Column<int>(type: "integer", nullable: false),
                    WeeklyAvailabilityJson = table.Column<string>(type: "text", nullable: false),
                    FrequentPartnersJson = table.Column<string>(type: "text", nullable: false),
                    BlockedPlayersJson = table.Column<string>(type: "text", nullable: false),
                    MatchesPlayed = table.Column<int>(type: "integer", nullable: false),
                    MatchesWon = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserRoleId",
                table: "Users",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Owners_UserRoleId",
                table: "Owners",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_UserRoleId",
                table: "Admins",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProfiles_UserId",
                table: "PlayerProfiles",
                column: "UserId",
                unique: true);

            // Create the foreign key constraints
            migrationBuilder.AddForeignKey(
                name: "FK_Admins_UserRoles_UserRoleId",
                table: "Admins",
                column: "UserRoleId",
                principalTable: "UserRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Owners_UserRoles_UserRoleId",
                table: "Owners",
                column: "UserRoleId",
                principalTable: "UserRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserRoles_UserRoleId",
                table: "Users",
                column: "UserRoleId",
                principalTable: "UserRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_UserRoles_UserRoleId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Owners_UserRoles_UserRoleId",
                table: "Owners");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserRoles_UserRoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "PlayerProfiles");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserRoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Owners_UserRoleId",
                table: "Owners");

            migrationBuilder.DropIndex(
                name: "IX_Admins_UserRoleId",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "UserRoleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserRoleId",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "UserRoleId",
                table: "Admins");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "UserRole");

            migrationBuilder.AddColumn<string>(
                name: "UserRole",
                table: "Owners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserRole",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
