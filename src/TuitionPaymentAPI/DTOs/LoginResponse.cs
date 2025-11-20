namespace TuitionPaymentAPI.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; } // in seconds
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
