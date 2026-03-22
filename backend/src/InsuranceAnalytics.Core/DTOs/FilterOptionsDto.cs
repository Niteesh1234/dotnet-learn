using InsuranceAnalytics.Core.Enums;

namespace InsuranceAnalytics.Core.DTOs;

public record FilterOptionsDto(
    IReadOnlyList<string> Regions,
    IReadOnlyList<PolicyType> PolicyTypes,
    IReadOnlyList<string> ClaimStatuses);