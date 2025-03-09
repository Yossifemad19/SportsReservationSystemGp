using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Repository.Migrations
{
    /// <inheritdoc />
    public partial class update_facility_booking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns for interval type
            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime_New",
                table: "Bookings",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime_New",
                table: "Bookings",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0));

            // Migrate data from old timestamp columns to new interval columns
            migrationBuilder.Sql(
                @"UPDATE ""Bookings"" 
          SET ""StartTime_New"" = ""StartTime"" - date_trunc('day', ""StartTime""),
              ""EndTime_New"" = ""EndTime"" - date_trunc('day', ""EndTime"");");

            // Drop old columns
            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Bookings");

            // Rename new columns to match original names
            migrationBuilder.RenameColumn(
                name: "StartTime_New",
                table: "Bookings",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "EndTime_New",
                table: "Bookings",
                newName: "EndTime");

            // Add OpeningTime and ClosingTime to Facilities
            migrationBuilder.AddColumn<TimeSpan>(
                name: "ClosingTime",
                table: "Facilities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OpeningTime",
                table: "Facilities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0));

            // Add Date column
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosingTime",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "OpeningTime",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Bookings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");
        }
    }
}
