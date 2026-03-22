using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Filters;
using InsuranceAnalytics.Core.Interfaces;
using InsuranceAnalytics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAnalytics.Infrastructure.Services;

public class KpiService(InsuranceAnalyticsDbContext db) : IKpiService
{
    public async Task<DashboardResponse> GetDashboardAsync(KpiFilter filter, CancellationToken cancellationToken = default)
    {
        var policies = await db.Policies.AsNoTracking().ToListAsync(cancellationToken);
        var claims = await db.Claims.AsNoTracking().Include(c => c.Policy).ToListAsync(cancellationToken);

        if (filter.FromDate is not null)
        {
            policies = policies.Where(x => x.StartDate >= filter.FromDate.Value).ToList();
            claims = claims.Where(x => x.ClaimDate >= filter.FromDate.Value).ToList();
        }

        if (filter.ToDate is not null)
        {
            policies = policies.Where(x => x.StartDate <= filter.ToDate.Value).ToList();
            claims = claims.Where(x => x.ClaimDate <= filter.ToDate.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(filter.Region))
        {
            policies = policies.Where(x => x.Region == filter.Region).ToList();
            claims = claims.Where(x => x.Policy is not null && x.Policy.Region == filter.Region).ToList();
        }

        if (filter.PolicyType is not null)
        {
            policies = policies.Where(x => x.PolicyType == filter.PolicyType.Value).ToList();
            claims = claims.Where(x => x.Policy is not null && x.Policy.PolicyType == filter.PolicyType.Value).ToList();
        }

        var totalPremium = policies.Sum(x => x.PremiumAmount);
        var totalClaims = claims.Sum(x => x.ClaimAmount);
        var policyCount = policies.Count;
        var claimCount = claims.Count;

        var lossRatio = totalPremium == 0 ? 0 : totalClaims / totalPremium;
        var claimFrequency = policyCount == 0 ? 0 : (decimal)claimCount / policyCount;

        var monthly = policies
            .GroupBy(x => new { x.StartDate.Year, x.StartDate.Month })
            .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
            .Select(g =>
            {
                var monthClaims = claims.Where(c => c.ClaimDate.Year == g.Key.Year && c.ClaimDate.Month == g.Key.Month).Sum(c => c.ClaimAmount);
                var premium = g.Sum(x => x.PremiumAmount);
                return new MonthlyTrendDto($"{g.Key.Year}-{g.Key.Month:D2}", premium, monthClaims, premium == 0 ? 0 : monthClaims / premium);
            })
            .ToList();

        var regionBreakdown = policies
            .GroupBy(x => x.Region)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var policyIds = g.Select(x => x.PolicyId).ToHashSet();
                var claimsTotal = claims.Where(c => policyIds.Contains(c.PolicyId)).Sum(c => c.ClaimAmount);
                return new RegionBreakdownDto(g.Key, g.Sum(x => x.PremiumAmount), claimsTotal);
            })
            .ToList();

        var kpis = new DashboardKpiDto(totalPremium, totalClaims, lossRatio, policyCount, claimFrequency);
        return new DashboardResponse(kpis, monthly, regionBreakdown);
    }
}
