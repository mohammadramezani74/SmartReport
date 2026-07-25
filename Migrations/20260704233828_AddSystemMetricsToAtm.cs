using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartReportLog.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemMetricsToAtm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CpuTemperatureC",
                table: "DailyAnalyses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CpuUsagePercent",
                table: "DailyAnalyses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiskTotalGb",
                table: "DailyAnalyses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiskUsedGb",
                table: "DailyAnalyses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RamTotalGb",
                table: "DailyAnalyses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RamUsedGb",
                table: "DailyAnalyses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CpuModel",
                table: "Atms",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OsVersion",
                table: "Atms",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CpuTemperatureC",
                table: "DailyAnalyses");

            migrationBuilder.DropColumn(
                name: "CpuUsagePercent",
                table: "DailyAnalyses");

            migrationBuilder.DropColumn(
                name: "DiskTotalGb",
                table: "DailyAnalyses");

            migrationBuilder.DropColumn(
                name: "DiskUsedGb",
                table: "DailyAnalyses");

            migrationBuilder.DropColumn(
                name: "RamTotalGb",
                table: "DailyAnalyses");

            migrationBuilder.DropColumn(
                name: "RamUsedGb",
                table: "DailyAnalyses");

            migrationBuilder.DropColumn(
                name: "CpuModel",
                table: "Atms");

            migrationBuilder.DropColumn(
                name: "OsVersion",
                table: "Atms");
        }
    }
}
