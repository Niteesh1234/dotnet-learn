using InsuranceAnalytics.Core.Enums;

namespace InsuranceAnalytics.Core.Entities;

public class Claim
{
    public int ClaimId { get; set; }
    public int PolicyId { get; set; }
    public decimal ClaimAmount { get; set; }
    public DateOnly ClaimDate { get; set; }
    public ClaimStatus Status { get; set; }

    public Policy? Policy { get; set; }
}
