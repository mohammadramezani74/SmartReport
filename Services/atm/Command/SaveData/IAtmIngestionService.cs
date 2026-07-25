using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartReportLog.Model.Atm.Submit;

namespace SmartReportLog.Services.atm.Command.SaveData
{
    public interface IAtmIngestionService
    {
        Task<IngestResult> IngestAsync(AtmScanDayDto days, CancellationToken ct);
    }
}
