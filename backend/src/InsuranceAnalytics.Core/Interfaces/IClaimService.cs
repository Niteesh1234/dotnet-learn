using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Filters;

namespace InsuranceAnalytics.Core.Interfaces;

public interface IClaimService
{
    Task<IReadOnlyList<ClaimResponse>> GetClaimsAsync(PagedRequest request, KpiFilter? filter = null, CancellationToken cancellationToken = default);
    Task<ClaimResponse> CreateClaimAsync(CreateClaimRequest request, CancellationToken cancellationToken = default);
}
