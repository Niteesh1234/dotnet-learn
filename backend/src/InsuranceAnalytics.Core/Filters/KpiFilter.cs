using InsuranceAnalytics.Core.Enums;

namespace InsuranceAnalytics.Core.Filters;

public class KpiFilter
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Region { get; set; }
    public PolicyType? PolicyType { get; set; }
}