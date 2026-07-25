using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartReportLog.Migrations
{
    /// <inheritdoc />
    public partial class addFieldGayaVersionToDailyReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GayaVersion",
                table: "DailyAnalyses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GayaVersion",
                table: "DailyAnalyses");
        }
    }
}
