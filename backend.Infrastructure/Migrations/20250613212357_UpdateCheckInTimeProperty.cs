using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCheckInTimeProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BlockEndDate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockEndDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "Users");
        }
    }
}
