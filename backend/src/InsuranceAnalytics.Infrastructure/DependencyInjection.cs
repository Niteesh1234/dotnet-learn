using InsuranceAnalytics.Core.Interfaces;
using InsuranceAnalytics.Infrastructure.Data;
using InsuranceAnalytics.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAnalytics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<InsuranceAnalyticsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));
        }
        else
        {
            services.AddDbContext<InsuranceAnalyticsDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("SqliteConnection")));
        }

        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IKpiService, KpiService>();
        services.AddScoped<IFilterService, FilterService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
