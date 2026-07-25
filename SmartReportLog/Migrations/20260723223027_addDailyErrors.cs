using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartReportLog.Migrations
{
    /// <inheritdoc />
    public partial class addDailyErrors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AtmTodayErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyAnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Device = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtmTodayErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtmTodayErrors_DailyAnalyses_DailyAnalysisId",
                        column: x => x.DailyAnalysisId,
                        principalTable: "DailyAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtmTodayErrors_DailyAnalysisId_Date_Device_ErrorCode",
                table: "AtmTodayErrors",
                columns: new[] { "DailyAnalysisId", "Date", "Device", "ErrorCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtmTodayErrors");
        }
    }
}
