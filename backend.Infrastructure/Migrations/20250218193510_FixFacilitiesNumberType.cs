using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixFacilitiesNumberType : Migration
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

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "OwnerProfiles",
                type: "varchar(11)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "OwnerProfiles",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "OwnerProfiles",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FacilitiesNumber",
                table: "OwnerProfiles",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FacilitiesLocation",
                table: "OwnerProfiles",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

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

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "OwnerProfile",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(11)");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "OwnerProfile",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "OwnerProfile",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "FacilitiesNumber",
                table: "OwnerProfile",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)");

            migrationBuilder.AlterColumn<string>(
                name: "FacilitiesLocation",
                table: "OwnerProfile",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

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
