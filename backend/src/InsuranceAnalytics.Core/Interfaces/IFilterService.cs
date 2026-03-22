using InsuranceAnalytics.Core.DTOs;

namespace InsuranceAnalytics.Core.Interfaces;

public interface IFilterService
{
    Task<FilterOptionsDto> GetFilterOptionsAsync(CancellationToken cancellationToken = default);
}
