using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Entities;
using InsuranceAnalytics.Core.Filters;
using InsuranceAnalytics.Core.Interfaces;
using InsuranceAnalytics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAnalytics.Infrastructure.Services;

public class ClaimService(InsuranceAnalyticsDbContext db) : IClaimService
{
    private IQueryable<Claim> ApplyFilter(IQueryable<Claim> query, KpiFilter? filter)
    {
        if (filter == null) return query;

        if (filter.FromDate is not null)
            query = query.Where(x => x.ClaimDate >= filter.FromDate.Value);

        if (filter.ToDate is not null)
            query = query.Where(x => x.ClaimDate <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Region))
            query = query.Where(x => x.Policy != null && x.Policy.Region == filter.Region);

        if (filter.PolicyType is not null)
            query = query.Where(x => x.Policy != null && x.Policy.PolicyType == filter.PolicyType.Value);

        return query;
    }

    public async Task<IReadOnlyList<ClaimResponse>> GetClaimsAsync(PagedRequest request, KpiFilter? filter = null, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(db.Claims.Include(x => x.Policy).AsNoTracking(), filter);

        return await query
            .OrderByDescending(x => x.ClaimDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ClaimResponse(x.ClaimId, x.PolicyId, x.Policy!.CustomerName, x.ClaimAmount, x.ClaimDate, x.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClaimResponse> CreateClaimAsync(CreateClaimRequest request, CancellationToken cancellationToken = default)
    {
        var policy = await db.Policies.FirstOrDefaultAsync(x => x.PolicyId == request.PolicyId, cancellationToken)
                     ?? throw new InvalidOperationException("Policy not found");

        var claim = new Claim
        {
            PolicyId = request.PolicyId,
            ClaimAmount = request.ClaimAmount,
            ClaimDate = request.ClaimDate,
            Status = request.Status
        };

        db.Claims.Add(claim);
        await db.SaveChangesAsync(cancellationToken);

        return new ClaimResponse(claim.ClaimId, claim.PolicyId, policy.CustomerName, claim.ClaimAmount, claim.ClaimDate, claim.Status);
    }
}
