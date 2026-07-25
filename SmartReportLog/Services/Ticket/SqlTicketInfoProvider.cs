using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartReportLog.Model.Ticket;
using SmartReportLog.Persistance;

namespace SmartReportLog.Services.Ticket
{
    public sealed class SqlTicketInfoProvider : ITicketInfoProvider
    {
        private const string Sql = @"
SELECT TOP 1
    e.RequestNo,
    RTRIM(e.CallDate)               AS CallDate,
    RTRIM(e.SerialNo)               AS SerialNo,
    RTRIM(v.MInvCode)               AS MInvCode,
    RTRIM(v.Name)                   AS DeviceName,
    RTRIM(v.CityName)               AS CityName,
    RTRIM(v.SupervisionStateName)   AS SupervisionStateName,
    v.StateCode,
    RTRIM(v.StateName)              AS StateName,
    RTRIM(v.CustmrName)             AS CustomerName,
    RTRIM(v.branchname)             AS BranchName,
    RTRIM(v.BranchCode)             AS BranchCode,
    RTRIM(a.ReferDate)              AS ReferDate,
    RTRIM(a.ReferEndTime)           AS ReferEndTime,
    RTRIM(o.AssignType)             AS AssignType,
    RTRIM(f.FullName)               AS TechName
FROM ErrorReport e
LEFT OUTER JOIN V_SerialInfo v
    ON v.ID = e.Ret16Id
LEFT OUTER JOIN OperationType o
    ON o.OperationTypeCode = e.OperationType
LEFT OUTER JOIN AfterRepair a
    ON a.RequestNo = e.RequestNo
   AND a.AfterRepairNo = (SELECT MAX(AfterRepairNo)
                          FROM AfterRepair
                          WHERE RequestNo = e.RequestNo)
LEFT OUTER JOIN V_FullName f
    ON f.PersonelNO = a.TechnesianNo
WHERE e.RequestNo = @ticketNo";

        private readonly ErDbContext _erDb;
        private readonly ILogger<SqlTicketInfoProvider> _logger;

        public SqlTicketInfoProvider(ErDbContext erDb, ILogger<SqlTicketInfoProvider> logger)
        {
            _erDb = erDb;
            _logger = logger;
        }

        public async Task<TicketLookupResult> GetByTicketNumberAsync(string? ticketNumber, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber))
                return new TicketLookupResult(false, "شماره تیکت ارسال نشده است.", null);

            if (!int.TryParse(ticketNumber.Trim(), out var ticketNo) || ticketNo <= 0)
                return new TicketLookupResult(false, "شماره تیکت نامعتبر است.", null);

            try
            {
                var row = await _erDb.TicketInfos
                    .FromSqlRaw(Sql, new SqlParameter("@ticketNo", ticketNo))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);

                if (row is null)
                    return new TicketLookupResult(false,
                        $"تیکتی با شماره {ticketNo} یافت نشد.", null);

                if (string.IsNullOrWhiteSpace(row.SerialNo))
                    return new TicketLookupResult(false,
                        $"برای تیکت {ticketNo} سریال معتبری یافت نشد.", null);

                if (string.IsNullOrWhiteSpace(row.MInvCode) && string.IsNullOrWhiteSpace(row.DeviceName))
                    return new TicketLookupResult(false,
                        $"برای سریال {row.SerialNo} اطلاعات دستگاه در سامانه یافت نشد.", null);

                NormalizeInPlace(row);
                return new TicketLookupResult(true, null, row);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اطلاعات تیکت {TicketNo}", ticketNo);
                return new TicketLookupResult(false,
                    "ارتباط با سامانه تیکت برقرار نشد.", null);
            }
        }

        private static void NormalizeInPlace(TicketInfoRow r)
        {
            r.DeviceName = Fa(r.DeviceName);
            r.CityName = Fa(r.CityName);
            r.SupervisionStateName = Fa(r.SupervisionStateName);
            r.StateName = Fa(r.StateName);
            r.CustomerName = Fa(r.CustomerName);
            r.BranchName = Fa(r.BranchName);
            r.TechName = Fa(r.TechName);
        }

        // ي و ك عربی را به ی و ک فارسی تبدیل می‌کند
        private static string? Fa(string? s) =>
            string.IsNullOrWhiteSpace(s)
                ? null
                : s.Trim().Replace('\u064A', '\u06CC').Replace('\u0643', '\u06A9');
    }
}
