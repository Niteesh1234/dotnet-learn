using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InsuranceAnalytics.Infrastructure.Services;

public class AuthService(IConfiguration configuration) : IAuthService
{
    public LoginResponse? Login(LoginRequest request)
    {
        var users = new Dictionary<string, (string Password, string Role)>(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = ("Admin@123", "Admin"),
            ["user"] = ("User@123", "User")
        };

        if (!users.TryGetValue(request.Username, out var user) || user.Password != request.Password)
            return null;

        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key missing");
        var issuer = configuration["Jwt:Issuer"] ?? "InsuranceAnalytics";
        var audience = configuration["Jwt:Audience"] ?? "InsuranceAnalyticsClient";
        var expiresMinutes = int.TryParse(configuration["Jwt:ExpiresMinutes"], out var value) ? value : 60;

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(expiresMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.Username),
            new(ClaimTypes.Name, request.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(issuer, audience, claims, notBefore: now, expires: expires, signingCredentials: credentials);
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), user.Role, expires);
    }
}
