using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Filters;

namespace InsuranceAnalytics.Core.Interfaces;

public interface IPolicyService
{
    Task<IReadOnlyList<PolicyResponse>> GetPoliciesAsync(PagedRequest request, KpiFilter? filter = null, CancellationToken cancellationToken = default);
    Task<PolicyResponse> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken cancellationToken = default);
    Task<int> SeedFakeDataAsync(int policyCount = 20, int maxClaimsPerPolicy = 3, CancellationToken cancellationToken = default);
}