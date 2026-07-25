using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartReportLog.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketInfoAndAtmLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BranchCode",
                table: "Atms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BranchName",
                table: "Atms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "Atms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Atms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "Atms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MInvCode",
                table: "Atms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StateCode",
                table: "Atms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateName",
                table: "Atms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupervisionStateName",
                table: "Atms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AtmTicketInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyAnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestNo = table.Column<int>(type: "int", nullable: false),
                    CallDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ReferDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ReferEndTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AssignType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TechName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FetchedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtmTicketInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtmTicketInfos_DailyAnalyses_DailyAnalysisId",
                        column: x => x.DailyAnalysisId,
                        principalTable: "DailyAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atms_BranchCode",
                table: "Atms",
                column: "BranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_Atms_StateCode",
                table: "Atms",
                column: "StateCode");

            migrationBuilder.CreateIndex(
                name: "IX_AtmTicketInfos_DailyAnalysisId",
                table: "AtmTicketInfos",
                column: "DailyAnalysisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AtmTicketInfos_RequestNo",
                table: "AtmTicketInfos",
                column: "RequestNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtmTicketInfos");

            migrationBuilder.DropIndex(
                name: "IX_Atms_BranchCode",
                table: "Atms");

            migrationBuilder.DropIndex(
                name: "IX_Atms_StateCode",
                table: "Atms");

            migrationBuilder.DropColumn(
                name: "BranchCode",
                table: "Atms");

            migrationBuilder.DropColumn(
                name: "BranchName",
                table: "Atms");

            migrationBuilder.DropColumn(
                name: "CityName",
                table: "Atms");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Atms");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "Atms");

            migrationBuilder.DropColumn(
                name: "MInvCode",
                table: "Atms");

            migrationBuilder.DropColumn(
                name: "StateCode",
                table: "Atms");

            migrationBuilder.DropColumn(
                name: "StateName",
                table: "Atms");

            migrationBuilder.DropColumn(
                name: "SupervisionStateName",
                table: "Atms");
        }
    }
}
