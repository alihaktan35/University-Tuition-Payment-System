namespace UniversityTuitionAPI.Services;

public interface IRateLimitService
{
    Task<bool> IsRateLimitExceededAsync(string studentNo, string endpoint, int maxRequests = 3);
    Task RecordRequestAsync(string studentNo, string endpoint);
}
