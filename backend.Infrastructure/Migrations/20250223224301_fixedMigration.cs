using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class fixedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OwnerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "varchar(10)", nullable: false),
                    LastName = table.Column<string>(type: "varchar(10)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(11)", nullable: false),
                    FacilitiesLocation = table.Column<string>(type: "varchar(50)", nullable: false),
                    FacilitiesNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    UserCredentialId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnerProfiles_UserCredentials_UserCredentialId",
                        column: x => x.UserCredentialId,
                        principalTable: "UserCredentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerProfiles_UserCredentialId",
                table: "OwnerProfiles",
                column: "UserCredentialId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OwnerProfiles");
        }
    }
}
