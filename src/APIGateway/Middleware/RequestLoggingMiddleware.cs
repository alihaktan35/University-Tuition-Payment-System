using System.Diagnostics;
using System.Text;

namespace APIGateway.Middleware;

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
        var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);

        // Log request details
        var requestLog = new StringBuilder();
        requestLog.AppendLine($"=== API GATEWAY REQUEST [{requestId}] ===");
        requestLog.AppendLine($"HTTP Method: {context.Request.Method}");
        requestLog.AppendLine($"Full Path: {context.Request.Path}{context.Request.QueryString}");
        requestLog.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        requestLog.AppendLine($"Source IP: {context.Connection.RemoteIpAddress}");

        // Log headers (excluding sensitive ones)
        var headers = context.Request.Headers
            .Where(h => !h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            .Select(h => $"{h.Key}={string.Join(",", h.Value.ToArray())}");
        requestLog.AppendLine($"Headers: {string.Join("; ", headers.ToList())}");

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        requestLog.AppendLine($"Authorization Header: {(authHeader != null ? "Present (Bearer Token)" : "Not Present")}");

        requestLog.AppendLine($"Request Size: {context.Request.ContentLength ?? 0} bytes");

        _logger.LogInformation(requestLog.ToString());

        // Capture original response stream
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Store request details for later
            context.Items["RequestStartTime"] = stopwatch.ElapsedMilliseconds;
            context.Items["RequestId"] = requestId;
            context.Items["HasAuthHeader"] = authHeader != null;

            // Call next middleware
            await _next(context);

            stopwatch.Stop();

            // Log response details
            LogResponse(context, stopwatch.ElapsedMilliseconds, requestId, authHeader != null);

            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "=== API GATEWAY ERROR [{RequestId}] ===\n" +
                "Exception: {ExceptionType}\n" +
                "Message: {ExceptionMessage}\n" +
                "Elapsed Time: {ElapsedMs} ms\n" +
                "Mapping Template Failure: YES",
                requestId, ex.GetType().Name, ex.Message, stopwatch.ElapsedMilliseconds);

            // Ensure response is set properly
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var errorJson = $"{{\"error\":{{\"code\":\"GATEWAY_ERROR\",\"message\":\"An error occurred in the API Gateway\",\"requestId\":\"{requestId}\"}}}}";
                await context.Response.WriteAsync(errorJson);
            }

            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private void LogResponse(HttpContext context, long elapsedMs, string requestId, bool hasAuthHeader)
    {
        var responseLog = new StringBuilder();
        responseLog.AppendLine($"=== API GATEWAY RESPONSE [{requestId}] ===");
        responseLog.AppendLine($"Status Code: {context.Response.StatusCode}");
        responseLog.AppendLine($"Response Latency: {elapsedMs} ms");
        responseLog.AppendLine($"Response Size: {context.Response.Body.Length} bytes");

        // Detailed authentication status based on status code
        var authStatus = DetermineAuthenticationStatus(context.Response.StatusCode, hasAuthHeader);
        responseLog.AppendLine($"Authentication Status: {authStatus}");

        // Check for rate limiting
        if (context.Response.StatusCode == 429)
        {
            responseLog.AppendLine("Rate Limit: EXCEEDED");
        }

        // Log mapping template failures (proxy errors)
        if (context.Response.StatusCode == 502 || context.Response.StatusCode == 503 || context.Response.StatusCode == 504)
        {
            responseLog.AppendLine("Mapping Template/Proxy Failure: YES");
            responseLog.AppendLine($"Failure Type: {GetProxyFailureType(context.Response.StatusCode)}");
        }

        _logger.LogInformation(responseLog.ToString());
    }

    private string DetermineAuthenticationStatus(int statusCode, bool hasAuthHeader)
    {
        return statusCode switch
        {
            401 when !hasAuthHeader => "FAILED - No Token Provided",
            401 when hasAuthHeader => "FAILED - Invalid/Expired Token",
            403 when hasAuthHeader => "SUCCEEDED but Insufficient Permissions (Forbidden)",
            403 when !hasAuthHeader => "FAILED - No Token for Protected Resource",
            429 => "N/A - Rate Limited",
            >= 200 and < 300 when hasAuthHeader => "SUCCEEDED - Valid Token",
            >= 200 and < 300 when !hasAuthHeader => "N/A - Public Endpoint",
            >= 400 and < 500 => "N/A - Client Error (Non-Auth)",
            >= 500 => "UNKNOWN - Server Error",
            _ => "UNKNOWN"
        };
    }

    private string GetProxyFailureType(int statusCode)
    {
        return statusCode switch
        {
            502 => "Bad Gateway - Backend returned invalid response",
            503 => "Service Unavailable - Backend not available",
            504 => "Gateway Timeout - Backend did not respond in time",
            _ => "Unknown proxy error"
        };
    }
}
