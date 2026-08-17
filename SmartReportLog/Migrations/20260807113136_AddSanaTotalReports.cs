using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartReportLog.Migrations
{
    /// <inheritdoc />
    public partial class AddSanaTotalReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AtmTotalReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TicketNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstLogDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastLogDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalCards = table.Column<int>(type: "int", nullable: false),
                    TotalTransactions = table.Column<int>(type: "int", nullable: false),
                    TotalReceipts = table.Column<int>(type: "int", nullable: false),
                    TotalReject = table.Column<int>(type: "int", nullable: false),
                    TotalDispenseRaw = table.Column<int>(type: "int", nullable: false),
                    TotalDispenseComputed = table.Column<int>(type: "int", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StoredFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DailyFileCount = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtmTotalReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtmTotalReports_Atms_AtmId",
                        column: x => x.AtmId,
                        principalTable: "Atms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AtmTotalCassettes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CassetteId = table.Column<int>(type: "int", nullable: false),
                    Denomination = table.Column<long>(type: "bigint", nullable: false),
                    InitialCount = table.Column<int>(type: "int", nullable: false),
                    TotalPickup = table.Column<int>(type: "int", nullable: false),
                    TotalDispense = table.Column<int>(type: "int", nullable: false),
                    TotalReject = table.Column<int>(type: "int", nullable: false),
                    LastKnownCount = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtmTotalCassettes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtmTotalCassettes_AtmTotalReports_TotalReportId",
                        column: x => x.TotalReportId,
                        principalTable: "AtmTotalReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AtmTotalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InfoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DenominationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtmTotalDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtmTotalDocuments_AtmTotalReports_TotalReportId",
                        column: x => x.TotalReportId,
                        principalTable: "AtmTotalReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AtmTotalErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Device = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtmTotalErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtmTotalErrors_AtmTotalReports_TotalReportId",
                        column: x => x.TotalReportId,
                        principalTable: "AtmTotalReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AtmTotalErrorDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalErrorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtmTotalErrorDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtmTotalErrorDates_AtmTotalErrors_TotalErrorId",
                        column: x => x.TotalErrorId,
                        principalTable: "AtmTotalErrors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtmTotalCassettes_TotalReportId_CassetteId",
                table: "AtmTotalCassettes",
                columns: new[] { "TotalReportId", "CassetteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AtmTotalDocuments_TotalReportId",
                table: "AtmTotalDocuments",
                column: "TotalReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AtmTotalErrorDates_TotalErrorId_Date",
                table: "AtmTotalErrorDates",
                columns: new[] { "TotalErrorId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AtmTotalErrors_Device_ErrorCode",
                table: "AtmTotalErrors",
                columns: new[] { "Device", "ErrorCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AtmTotalErrors_TotalReportId",
                table: "AtmTotalErrors",
                column: "TotalReportId");

            migrationBuilder.CreateIndex(
                name: "IX_AtmTotalReports_AtmId",
                table: "AtmTotalReports",
                column: "AtmId");

            migrationBuilder.CreateIndex(
                name: "IX_AtmTotalReports_FirstLogDate_LastLogDate",
                table: "AtmTotalReports",
                columns: new[] { "FirstLogDate", "LastLogDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AtmTotalReports_SerialNumber",
                table: "AtmTotalReports",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AtmTotalReports_TicketNumber",
                table: "AtmTotalReports",
                column: "TicketNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AtmTotalReports_TicketNumber_FirstLogDate_LastLogDate",
                table: "AtmTotalReports",
                columns: new[] { "TicketNumber", "FirstLogDate", "LastLogDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtmTotalCassettes");

            migrationBuilder.DropTable(
                name: "AtmTotalDocuments");

            migrationBuilder.DropTable(
                name: "AtmTotalErrorDates");

            migrationBuilder.DropTable(
                name: "AtmTotalErrors");

            migrationBuilder.DropTable(
                name: "AtmTotalReports");
        }
    }
}
