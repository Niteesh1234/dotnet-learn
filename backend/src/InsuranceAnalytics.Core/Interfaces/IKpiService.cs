using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Filters;

namespace InsuranceAnalytics.Core.Interfaces;

public interface IKpiService
{
    Task<DashboardResponse> GetDashboardAsync(KpiFilter filter, CancellationToken cancellationToken = default);
}
