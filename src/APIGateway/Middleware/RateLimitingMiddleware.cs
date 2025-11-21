using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using APIGateway.Data;
using APIGateway.Models;

namespace APIGateway.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, GatewayDbContext dbContext)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // Only apply rate limiting to mobile app endpoint (no authentication required)
        if (path.Contains("/api/v1/tuition/query/"))
        {
            var segments = path.Split('/');
            var studentNo = segments.Length > 0 ? segments[^1] : null;

            if (!string.IsNullOrEmpty(studentNo))
            {
                var maxRequests = _configuration.GetValue<int>("RateLimiting:MaxRequestsPerDay", 3);
                var today = DateTime.UtcNow.Date;

                _logger.LogInformation(
                    "Rate limit check for student: {StudentNo}, endpoint: {Path}, date: {Date}",
                    studentNo, path, today);

                var rateLimit = await dbContext.RateLimits
                    .FirstOrDefaultAsync(r => r.StudentNo == studentNo &&
                                            r.Endpoint == path &&
                                            r.Date == today);

                if (rateLimit == null)
                {
                    // Create new rate limit record
                    rateLimit = new RateLimit
                    {
                        StudentNo = studentNo,
                        Endpoint = path,
                        CallCount = 1,
                        Date = today,
                        LastCall = DateTime.UtcNow
                    };
                    dbContext.RateLimits.Add(rateLimit);
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation(
                        "Rate limit initialized for student: {StudentNo}, count: 1/{Max}",
                        studentNo, maxRequests);
                }
                else
                {
                    if (rateLimit.CallCount >= maxRequests)
                    {
                        _logger.LogWarning(
                            "Rate limit EXCEEDED for student: {StudentNo}, calls: {Count}/{Max}",
                            studentNo, rateLimit.CallCount, maxRequests);

                        context.Response.StatusCode = 429; // Too Many Requests
                        context.Response.ContentType = "application/json";

                        var errorResponse = new
                        {
                            error = new
                            {
                                code = "RATE_LIMIT_EXCEEDED",
                                message = $"You have exceeded the maximum of {maxRequests} requests per day",
                                details = new
                                {
                                    studentNo = studentNo,
                                    callsToday = rateLimit.CallCount,
                                    maxAllowed = maxRequests,
                                    resetTime = today.AddDays(1).ToString("yyyy-MM-ddT00:00:00Z")
                                }
                            }
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                        return;
                    }

                    // Increment call count
                    rateLimit.CallCount++;
                    rateLimit.LastCall = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation(
                        "Rate limit updated for student: {StudentNo}, count: {Count}/{Max}",
                        studentNo, rateLimit.CallCount, maxRequests);
                }
            }
        }

        await _next(context);
    }
}
