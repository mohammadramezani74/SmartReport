namespace SmartReportLog.Model.Atm.Queries
{
    public record AtmTotalDetailDto(
        Guid ReportId,
        string TicketNumber,
        string SerialNumber,
        DateOnly FirstLogDate,
        DateOnly LastLogDate,
        DateTime? ExportDate,
        DateTime UploadedAt,
        string? BankName,
        int TotalCards,
        int TotalTransactions,
        int TotalReceipts,
        int TotalReject,
        int TotalDispense,
        int DailyFileCount,
        long FileSizeBytes,
        List<AtmTotalCassetteDto> Cassettes,
        List<AtmTotalErrorDto> Errors)
    {
        public int CoveredDays => LastLogDate.DayNumber - FirstLogDate.DayNumber + 1;
    }

    public record AtmTotalCassetteDto(
        int CassetteId, long Denomination, int InitialCount,
        int TotalPickup, int TotalDispense, int TotalReject, int LastKnownCount)
    {
        public double RejectRate => TotalPickup == 0 ? 0 : (double)TotalReject / TotalPickup * 100;
    }

    public record AtmTotalErrorDto(
        string Device, string ErrorCode, string? Description, int Count,
        List<AtmTotalErrorPointDto> Trend);

    public record AtmTotalErrorPointDto(DateOnly Date, int Count);
}