using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Add request/response logging middleware
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();

    // Log request
    var requestLog = new StringBuilder();
    requestLog.AppendLine("=== API GATEWAY - INCOMING REQUEST ===");
    requestLog.AppendLine($"HTTP Method: {context.Request.Method}");
    requestLog.AppendLine($"Path: {context.Request.Path}{context.Request.QueryString}");
    requestLog.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
    requestLog.AppendLine($"Source IP: {context.Connection.RemoteIpAddress}");
    requestLog.AppendLine($"Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
    requestLog.AppendLine($"Content Length: {context.Request.ContentLength ?? 0} bytes");

    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    requestLog.AppendLine($"Authentication: {(authHeader != null ? "Token Present" : "No Token")}");

    Console.WriteLine(requestLog.ToString());

    // Capture original response stream
    var originalBodyStream = context.Response.Body;

    try
    {
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await next(context);

        stopwatch.Stop();

        // Log response
        var responseLog = new StringBuilder();
        responseLog.AppendLine("=== API GATEWAY - OUTGOING RESPONSE ===");
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
        else if (context.Response.StatusCode == 429)
        {
            responseLog.AppendLine("Rate Limit: EXCEEDED");
        }
        else if (authHeader != null)
        {
            responseLog.AppendLine("Authentication: SUCCEEDED");
        }

        Console.WriteLine(responseLog.ToString());

        // Copy response back to original stream
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }
    finally
    {
        context.Response.Body = originalBodyStream;
    }
});

app.UseCors();

// Map reverse proxy routes
app.MapReverseProxy();

app.Run();
