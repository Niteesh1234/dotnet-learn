using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Entities;
using InsuranceAnalytics.Core.Enums;
using InsuranceAnalytics.Core.Filters;
using InsuranceAnalytics.Core.Interfaces;
using InsuranceAnalytics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAnalytics.Infrastructure.Services;

public class PolicyService(InsuranceAnalyticsDbContext db) : IPolicyService
{
    private IQueryable<Policy> ApplyFilter(IQueryable<Policy> query, KpiFilter? filter)
    {
        if (filter == null) return query;

        if (filter.FromDate is not null)
            query = query.Where(x => x.StartDate >= filter.FromDate.Value);

        if (filter.ToDate is not null)
            query = query.Where(x => x.StartDate <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Region))
            query = query.Where(x => x.Region == filter.Region);

        if (filter.PolicyType is not null)
            query = query.Where(x => x.PolicyType == filter.PolicyType.Value);

        return query;
    }

    public async Task<IReadOnlyList<PolicyResponse>> GetPoliciesAsync(PagedRequest request, KpiFilter? filter = null, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(db.Policies.Include(x => x.Claims).AsNoTracking(), filter);

        return await query
            .OrderBy(x => x.PolicyId)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new PolicyResponse(x.PolicyId, x.CustomerName, x.PolicyType, x.PremiumAmount, x.StartDate, x.EndDate, x.Region, x.Claims.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<PolicyResponse> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var policy = new Policy
        {
            CustomerName = request.CustomerName,
            PolicyType = request.PolicyType,
            PremiumAmount = request.PremiumAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Region = request.Region
        };

        db.Policies.Add(policy);
        await db.SaveChangesAsync(cancellationToken);

        return new PolicyResponse(policy.PolicyId, policy.CustomerName, policy.PolicyType, policy.PremiumAmount, policy.StartDate, policy.EndDate, policy.Region, 0);
    }

    public async Task<int> SeedFakeDataAsync(int policyCount = 20, int maxClaimsPerPolicy = 3, CancellationToken cancellationToken = default)
    {
        var faker = new Bogus.Faker("en");
        var regions = new[] { "West", "South", "North", "East", "Central" };
        var policyTypes = Enum.GetValues<PolicyType>().ToArray();
        var claimStatuses = Enum.GetValues<ClaimStatus>().ToArray();

        var newPolicies = new List<Policy>();

        for (var i = 0; i < policyCount; i++)
        {
            var startDate = faker.Date.Between(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow.AddMonths(1)).Date;
            var endDate = startDate.AddYears(1);

            newPolicies.Add(new Policy
            {
                CustomerName = faker.Name.FullName(),
                PolicyType = policyTypes[faker.Random.Int(0, policyTypes.Length - 1)],
                PremiumAmount = Math.Round(faker.Finance.Amount(500, 5000), 2),
                StartDate = DateOnly.FromDateTime(startDate),
                EndDate = DateOnly.FromDateTime(endDate),
                Region = regions[faker.Random.Int(0, regions.Length - 1)]
            });
        }

        await db.Policies.AddRangeAsync(newPolicies, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var newClaims = new List<Claim>();
        foreach (var policy in newPolicies)
        {
            var claimCount = faker.Random.Int(0, maxClaimsPerPolicy);
            for (var j = 0; j < claimCount; j++)
            {
                var claimDate = faker.Date.Between(policy.StartDate.ToDateTime(TimeOnly.MinValue), DateTime.UtcNow);
                newClaims.Add(new Claim
                {
                    PolicyId = policy.PolicyId,
                    ClaimAmount = Math.Round(faker.Finance.Amount(100, 3000), 2),
                    ClaimDate = DateOnly.FromDateTime(claimDate),
                    Status = claimStatuses[faker.Random.Int(0, claimStatuses.Length - 1)]
                });
            }
        }

        if (newClaims.Any())
        {
            await db.Claims.AddRangeAsync(newClaims, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        return newPolicies.Count;
    }
}
