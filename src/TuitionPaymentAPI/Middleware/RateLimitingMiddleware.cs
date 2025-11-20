using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TuitionPaymentAPI.Data;
using TuitionPaymentAPI.Models;

namespace TuitionPaymentAPI.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public RateLimitingMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context, TuitionDbContext dbContext)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // Only apply rate limiting to mobile app endpoint
        if (path.Contains("/api/v1/tuition/query/"))
        {
            var segments = path.Split('/');
            var studentNo = segments.Length > 0 ? segments[^1] : null;

            if (!string.IsNullOrEmpty(studentNo))
            {
                var maxRequests = _configuration.GetValue<int>("RateLimiting:MaxRequestsPerDay", 3);
                var today = DateTime.UtcNow.Date;

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
                }
                else
                {
                    if (rateLimit.CallCount >= maxRequests)
                    {
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
                }
            }
        }

        await _next(context);
    }
}
