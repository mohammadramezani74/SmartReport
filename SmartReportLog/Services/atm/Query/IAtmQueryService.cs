using SmartReportLog.Model.Atm.Queries;

namespace SmartReportLog.Services.atm.Query
{
    public interface IAtmQueryService
    {
        Task<(List<AtmListItemDto> Items, int TotalCount)> GetAtmListAsync(string? search, int page, int pageSize, CancellationToken ct);
        Task<(List<AtmPeriodListItemDto> Items, int TotalCount)> GetAtmPeriodsAsync(
    Guid atmId, int page, int pageSize, CancellationToken ct);
        Task<AtmDetailDto?> GetAtmDetailAsync(Guid atmId, DateOnly? from, DateOnly? to, CancellationToken ct);
        Task<AtmDetailDto?> GetAtmPeriodDetailAsync(Guid atmId, Guid periodId, CancellationToken ct);
        Task<List<AtmErrorCountDto>> TopTenAtmWithMostErrors( CancellationToken ct);
        Task<AtmDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken);
        Task<List<AtmErrorTypeDistributionDto>> GetErrorTypeDistributionAsync(CancellationToken cancellationToken);
    }
}

