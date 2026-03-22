using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAnalytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = authService.Login(request);
        if (result is null) return Unauthorized(new { message = "Invalid username or password" });
        return Ok(result);
    }
}
