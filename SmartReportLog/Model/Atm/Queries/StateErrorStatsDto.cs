namespace SmartReportLog.Model.Atm.Queries
{
    public record StateErrorStatsDto(int? StateCode, string StateName, int AtmCount, int ErrorCount)
    {
        public double ErrorsPerAtm => AtmCount == 0 ? 0 : (double)ErrorCount / AtmCount;
    }
}
