namespace SmartReportLog.Model.Atm.Submit
{
    public sealed record IngestResult(bool Success, string Message, string? Warning = null);
}
