using InsuranceAnalytics.Core.DTOs;

namespace InsuranceAnalytics.Core.Interfaces;

public interface IAuthService
{
    LoginResponse? Login(LoginRequest request);
}
