using SmartReportLog.Model.Ticket;

namespace SmartReportLog.Model.Atm.Queries
{
    public record AtmDetailDto(
     Guid Id, string SerialNumber, string? CpuModel, string? OsVersion,
     List<AtmPeriodPointDto> Periods,
     List<AtmErrorSummaryDto> ErrorsAggregated,
     List<AtmCassetteSummaryDto> CassettesLatest,
     List<AtmTodayErrorDto> TodayErrors,
     int CpuUsagePercent, int RamTotalGb, int RamUsedGb, int? CpuTemperatureC,
     int DiskTotalGb, int DiskUsedGb, string gayaversion, string ImageVersion,
      AtmLocationDto? Location = null,
 AtmTicketDetailDto? Ticket = null);
}
