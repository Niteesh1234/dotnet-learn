namespace InsuranceAnalytics.Core.DTOs;

public record DashboardKpiDto(
    decimal TotalPremium,
    decimal TotalClaims,
    decimal LossRatio,
    int PolicyCount,
    decimal ClaimFrequency);

public record MonthlyTrendDto(string Month, decimal Premium, decimal Claims, decimal LossRatio);
public record RegionBreakdownDto(string Region, decimal Premium, decimal Claims);

public record DashboardResponse(
    DashboardKpiDto Kpis,
    IReadOnlyList<MonthlyTrendDto> MonthlyTrends,
    IReadOnlyList<RegionBreakdownDto> RegionBreakdown);