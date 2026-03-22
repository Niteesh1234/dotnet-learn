using InsuranceAnalytics.Core.Enums;

namespace InsuranceAnalytics.Core.DTOs;

public record CreatePolicyRequest(
    string CustomerName,
    PolicyType PolicyType,
    decimal PremiumAmount,
    DateOnly StartDate,
    DateOnly EndDate,
    string Region);

public record PolicyResponse(
    int PolicyId,
    string CustomerName,
    PolicyType PolicyType,
    decimal PremiumAmount,
    DateOnly StartDate,
    DateOnly EndDate,
    string Region,
    int ClaimCount);