using InsuranceAnalytics.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAnalytics.Infrastructure.Data;

public class InsuranceAnalyticsDbContext(DbContextOptions<InsuranceAnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Claim> Claims => Set<Claim>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(x => x.PolicyId);
            entity.Property(x => x.CustomerName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Region).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PremiumAmount).HasPrecision(18, 2);
            entity.HasIndex(x => x.Region);
            entity.HasIndex(x => x.StartDate);
        });

        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(x => x.ClaimId);
            entity.Property(x => x.ClaimAmount).HasPrecision(18, 2);
            entity.HasIndex(x => x.PolicyId);
            entity.HasIndex(x => x.ClaimDate);
            entity.HasOne(x => x.Policy)
                .WithMany(x => x.Claims)
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
