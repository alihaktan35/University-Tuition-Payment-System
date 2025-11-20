using System.Diagnostics;
using System.Text;

namespace TuitionPaymentAPI.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Log request details
        var requestLog = new StringBuilder();
        requestLog.AppendLine("=== INCOMING REQUEST ===");
        requestLog.AppendLine($"HTTP Method: {context.Request.Method}");
        requestLog.AppendLine($"Path: {context.Request.Path}{context.Request.QueryString}");
        requestLog.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        requestLog.AppendLine($"Source IP: {context.Connection.RemoteIpAddress}");
        requestLog.AppendLine($"Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
        requestLog.AppendLine($"Content Length: {context.Request.ContentLength ?? 0} bytes");

        // Check if authentication succeeded
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        requestLog.AppendLine($"Authentication: {(authHeader != null ? "Token Present" : "No Token")}");

        _logger.LogInformation(requestLog.ToString());

        // Capture the original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            // Log response details
            var responseLog = new StringBuilder();
            responseLog.AppendLine("=== OUTGOING RESPONSE ===");
            responseLog.AppendLine($"Status Code: {context.Response.StatusCode}");
            responseLog.AppendLine($"Response Time: {stopwatch.ElapsedMilliseconds} ms");
            responseLog.AppendLine($"Response Size: {responseBody.Length} bytes");

            if (context.Response.StatusCode == 401)
            {
                responseLog.AppendLine("Authentication: FAILED - Unauthorized");
            }
            else if (context.Response.StatusCode == 403)
            {
                responseLog.AppendLine("Authentication: Token Valid but Insufficient Permissions");
            }
            else if (authHeader != null)
            {
                responseLog.AppendLine("Authentication: SUCCEEDED");
            }

            _logger.LogInformation(responseLog.ToString());

            // Copy the response body back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}
