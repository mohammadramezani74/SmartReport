using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartReportLog.Migrations
{
    /// <inheritdoc />
    public partial class addEndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "DailyAnalyses",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "DailyAnalyses");
        }
    }
}
