using InsuranceAnalytics.Core.Entities;
using InsuranceAnalytics.Core.Enums;

namespace InsuranceAnalytics.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(InsuranceAnalyticsDbContext db, CancellationToken cancellationToken = default)
    {
        if (db.Policies.Any())
        {
            // Keep adding fake rows on every startup for ongoing development testing.
            AddFakeData(db, cancellationToken);
            return;
        }

        var policies = new List<Policy>
        {
            new() { CustomerName = "John Carter", PolicyType = PolicyType.Auto, PremiumAmount = 1200, StartDate = new DateOnly(2025,1,1), EndDate = new DateOnly(2025,12,31), Region = "West" },
            new() { CustomerName = "Maya Singh", PolicyType = PolicyType.Health, PremiumAmount = 2400, StartDate = new DateOnly(2025,2,1), EndDate = new DateOnly(2026,1,31), Region = "South" },
            new() { CustomerName = "Robert Lee", PolicyType = PolicyType.Life, PremiumAmount = 1800, StartDate = new DateOnly(2025,3,1), EndDate = new DateOnly(2026,2,28), Region = "North" },
            new() { CustomerName = "Alina Rose", PolicyType = PolicyType.Home, PremiumAmount = 2100, StartDate = new DateOnly(2025,4,1), EndDate = new DateOnly(2026,3,31), Region = "East" }
        };

        await db.Policies.AddRangeAsync(policies, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new() { PolicyId = policies[0].PolicyId, ClaimAmount = 250, ClaimDate = new DateOnly(2025,3,10), Status = ClaimStatus.Closed },
            new() { PolicyId = policies[1].PolicyId, ClaimAmount = 800, ClaimDate = new DateOnly(2025,6,2), Status = ClaimStatus.Open },
            new() { PolicyId = policies[2].PolicyId, ClaimAmount = 300, ClaimDate = new DateOnly(2025,8,21), Status = ClaimStatus.Pending },
            new() { PolicyId = policies[0].PolicyId, ClaimAmount = 150, ClaimDate = new DateOnly(2025,10,5), Status = ClaimStatus.Closed }
        };

        await db.Claims.AddRangeAsync(claims, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Add extra fake data (guarantees ongoing database growth for local/dev iterations)
        AddFakeData(db, cancellationToken);
    }

    private static void AddFakeData(InsuranceAnalyticsDbContext db, CancellationToken cancellationToken)
    {
        const int additionalPolicies = 20;
        const int maxClaimsPerPolicy = 3;

        var faker = new Bogus.Faker("en");
        var regions = new[] { "West", "South", "North", "East", "Central" };
        var policyTypes = Enum.GetValues<PolicyType>().ToArray();
        var claimStatuses = Enum.GetValues<ClaimStatus>().ToArray();

        var generatedPolicies = new List<Policy>();

        for (var i = 0; i < additionalPolicies; i++)
        {
            var startDate = faker.Date.Between(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow.AddMonths(3)).Date;
            var endDate = startDate.AddYears(1);

            generatedPolicies.Add(new Policy
            {
                CustomerName = faker.Name.FullName(),
                PolicyType = policyTypes[faker.Random.Int(0, policyTypes.Length - 1)],
                PremiumAmount = Math.Round(faker.Finance.Amount(500, 5000), 2),
                StartDate = DateOnly.FromDateTime(startDate),
                EndDate = DateOnly.FromDateTime(endDate),
                Region = regions[faker.Random.Int(0, regions.Length - 1)]
            });
        }

        db.Policies.AddRange(generatedPolicies);
        db.SaveChanges();

        var generatedClaims = new List<Claim>();
        foreach (var policy in generatedPolicies)
        {
            var claimCount = faker.Random.Int(0, maxClaimsPerPolicy);
            for (var j = 0; j < claimCount; j++)
            {
                var claimDate = faker.Date.Between(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow);
                generatedClaims.Add(new Claim
                {
                    PolicyId = policy.PolicyId,
                    ClaimAmount = Math.Round(faker.Finance.Amount(100, 3000), 2),
                    ClaimDate = DateOnly.FromDateTime(claimDate),
                    Status = claimStatuses[faker.Random.Int(0, claimStatuses.Length - 1)]
                });
            }
        }

        if (generatedClaims.Any())
        {
            db.Claims.AddRange(generatedClaims);
            db.SaveChanges();
        }
    }
}
