using System.Diagnostics;
using System.Text;
using UniversityTuitionAPI.Data;
using UniversityTuitionAPI.Models;

namespace UniversityTuitionAPI.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestTimestamp = DateTime.UtcNow;

        // Capture request details
        var requestPath = context.Request.Path + context.Request.QueryString;
        var httpMethod = context.Request.Method;
        var sourceIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var requestHeaders = SerializeHeaders(context.Request.Headers);

        // Capture request body size
        context.Request.EnableBuffering();
        var requestSize = context.Request.ContentLength ?? 0;

        // Store original response body stream
        var originalResponseBody = context.Response.Body;

        try
        {
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Call the next middleware
            await _next(context);

            stopwatch.Stop();

            // Capture response details
            var statusCode = context.Response.StatusCode;
            var responseSize = context.Response.ContentLength ?? responseBodyStream.Length;
            var latencyMs = stopwatch.ElapsedMilliseconds;

            // Check if authentication succeeded
            var authSucceeded = context.User?.Identity?.IsAuthenticated ?? false;

            // Log to database
            var apiLog = new ApiLog
            {
                HttpMethod = httpMethod,
                RequestPath = requestPath,
                RequestTimestamp = requestTimestamp,
                SourceIpAddress = sourceIp,
                RequestHeaders = requestHeaders,
                RequestSize = requestSize,
                AuthenticationSucceeded = authSucceeded,
                StatusCode = statusCode,
                ResponseLatencyMs = latencyMs,
                ResponseSize = responseSize
            };

            dbContext.ApiLogs.Add(apiLog);
            await dbContext.SaveChangesAsync();

            // Log to console/file
            _logger.LogInformation(
                "HTTP {Method} {Path} - Status: {StatusCode} - Latency: {Latency}ms - Auth: {Auth}",
                httpMethod, requestPath, statusCode, latencyMs, authSucceeded);

            // Copy response back to original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBody);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log error
            var apiLog = new ApiLog
            {
                HttpMethod = httpMethod,
                RequestPath = requestPath,
                RequestTimestamp = requestTimestamp,
                SourceIpAddress = sourceIp,
                RequestHeaders = requestHeaders,
                RequestSize = requestSize,
                AuthenticationSucceeded = false,
                StatusCode = 500,
                ResponseLatencyMs = stopwatch.ElapsedMilliseconds,
                ResponseSize = 0,
                ErrorMessage = ex.Message
            };

            dbContext.ApiLogs.Add(apiLog);
            await dbContext.SaveChangesAsync();

            _logger.LogError(ex, "Error processing request {Method} {Path}", httpMethod, requestPath);

            throw;
        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }

    private string SerializeHeaders(IHeaderDictionary headers)
    {
        var sb = new StringBuilder();
        foreach (var header in headers)
        {
            // Skip sensitive headers
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                continue;

            sb.AppendLine($"{header.Key}: {header.Value}");
        }
        return sb.ToString();
    }
}
