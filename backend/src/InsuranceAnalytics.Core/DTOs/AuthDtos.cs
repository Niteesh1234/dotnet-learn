namespace InsuranceAnalytics.Core.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string Role, DateTime ExpiresAtUtc);