using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartReportLog.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Atms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalCards = table.Column<int>(type: "int", nullable: false),
                    TotalTransactions = table.Column<int>(type: "int", nullable: false),
                    ReceiptCount = table.Column<int>(type: "int", nullable: false),
                    AuiSeconds = table.Column<double>(type: "float", nullable: false),
                    DailyDispenseTotal = table.Column<int>(type: "int", nullable: false),
                    DailyRejectTotal = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyAnalyses_Atms_AtmId",
                        column: x => x.AtmId,
                        principalTable: "Atms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyCassettes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyAnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CassetteNumber = table.Column<int>(type: "int", nullable: false),
                    Denomination = table.Column<long>(type: "bigint", nullable: false),
                    InitialCount = table.Column<int>(type: "int", nullable: false),
                    FinalCount = table.Column<int>(type: "int", nullable: false),
                    TotalPickup = table.Column<int>(type: "int", nullable: false),
                    TotalDispense = table.Column<int>(type: "int", nullable: false),
                    TotalReject = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyCassettes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyCassettes_DailyAnalyses_DailyAnalysisId",
                        column: x => x.DailyAnalysisId,
                        principalTable: "DailyAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyHardwareErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyAnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Device = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyHardwareErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyHardwareErrors_DailyAnalyses_DailyAnalysisId",
                        column: x => x.DailyAnalysisId,
                        principalTable: "DailyAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atms_SerialNumber",
                table: "Atms",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyAnalyses_AtmId_Date",
                table: "DailyAnalyses",
                columns: new[] { "AtmId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyCassettes_DailyAnalysisId",
                table: "DailyCassettes",
                column: "DailyAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyHardwareErrors_DailyAnalysisId_Device_ErrorCode",
                table: "DailyHardwareErrors",
                columns: new[] { "DailyAnalysisId", "Device", "ErrorCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyCassettes");

            migrationBuilder.DropTable(
                name: "DailyHardwareErrors");

            migrationBuilder.DropTable(
                name: "DailyAnalyses");

            migrationBuilder.DropTable(
                name: "Atms");
        }
    }
}
