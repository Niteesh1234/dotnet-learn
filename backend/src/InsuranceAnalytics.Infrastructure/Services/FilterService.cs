using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Enums;
using InsuranceAnalytics.Core.Interfaces;
using InsuranceAnalytics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAnalytics.Infrastructure.Services;

public class FilterService(InsuranceAnalyticsDbContext db) : IFilterService
{
    public async Task<FilterOptionsDto> GetFilterOptionsAsync(CancellationToken cancellationToken = default)
    {
        var regions = await db.Policies.AsNoTracking().Select(x => x.Region).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);
        var policyTypes = Enum.GetValues<PolicyType>().ToList();
        var claimStatuses = Enum.GetNames<ClaimStatus>().ToList();

        return new FilterOptionsDto(regions, policyTypes, claimStatuses);
    }
}
