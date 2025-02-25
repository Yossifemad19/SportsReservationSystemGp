using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class SyncChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OwnerProfile_UserCredentials_UserCredentialId",
                table: "OwnerProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnerProfile",
                table: "OwnerProfile");

            migrationBuilder.RenameTable(
                name: "OwnerProfile",
                newName: "OwnerProfiles");

            migrationBuilder.RenameIndex(
                name: "IX_OwnerProfile_UserCredentialId",
                table: "OwnerProfiles",
                newName: "IX_OwnerProfiles_UserCredentialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnerProfiles",
                table: "OwnerProfiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OwnerProfiles_UserCredentials_UserCredentialId",
                table: "OwnerProfiles",
                column: "UserCredentialId",
                principalTable: "UserCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OwnerProfiles_UserCredentials_UserCredentialId",
                table: "OwnerProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnerProfiles",
                table: "OwnerProfiles");

            migrationBuilder.RenameTable(
                name: "OwnerProfiles",
                newName: "OwnerProfile");

            migrationBuilder.RenameIndex(
                name: "IX_OwnerProfiles_UserCredentialId",
                table: "OwnerProfile",
                newName: "IX_OwnerProfile_UserCredentialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnerProfile",
                table: "OwnerProfile",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OwnerProfile_UserCredentials_UserCredentialId",
                table: "OwnerProfile",
                column: "UserCredentialId",
                principalTable: "UserCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
