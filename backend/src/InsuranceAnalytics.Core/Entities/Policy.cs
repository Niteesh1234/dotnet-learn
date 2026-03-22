using InsuranceAnalytics.Core.Enums;

namespace InsuranceAnalytics.Core.Entities;

public class Policy
{
    public int PolicyId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public PolicyType PolicyType { get; set; }
    public decimal PremiumAmount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Region { get; set; } = string.Empty;

    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
