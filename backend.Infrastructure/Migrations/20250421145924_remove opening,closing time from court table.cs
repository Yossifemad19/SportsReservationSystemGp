using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class removeopeningclosingtimefromcourttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosingTime",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "OpeningTime",
                table: "Courts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "ClosingTime",
                table: "Courts",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OpeningTime",
                table: "Courts",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
