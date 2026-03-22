using InsuranceAnalytics.Core.Enums;

namespace InsuranceAnalytics.Core.DTOs;

public record CreateClaimRequest(int PolicyId, decimal ClaimAmount, DateOnly ClaimDate, ClaimStatus Status);

public record ClaimResponse(
    int ClaimId,
    int PolicyId,
    string CustomerName,
    decimal ClaimAmount,
    DateOnly ClaimDate,
    ClaimStatus Status);