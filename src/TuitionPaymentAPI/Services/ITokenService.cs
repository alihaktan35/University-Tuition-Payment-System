using TuitionPaymentAPI.Models;

namespace TuitionPaymentAPI.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
