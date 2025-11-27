using UniversityTuitionAPI.DTOs;

namespace UniversityTuitionAPI.Services;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request);
    string GenerateJwtToken(string username, string role);
}
