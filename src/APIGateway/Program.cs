using Microsoft.EntityFrameworkCore;
using APIGateway.Data;
using APIGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Entity Framework - SQLite for Development, SQL Server for Production
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (builder.Environment.IsProduction() && connectionString!.Contains("database.windows.net"))
{
    // Use SQL Server for Azure production
    builder.Services.AddDbContext<GatewayDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Use SQLite for local development
    builder.Services.AddDbContext<GatewayDbContext>(options =>
        options.UseSqlite(connectionString));
}

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

// Apply database migrations for Gateway
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GatewayDbContext>();
        context.Database.Migrate();
        app.Logger.LogInformation("Gateway database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the Gateway database");
    }
}

app.UseCors();

// CRITICAL: Rate limiting must be applied BEFORE reverse proxy
// This ensures rate limits are enforced at the gateway level
app.UseMiddleware<RateLimitingMiddleware>();

// Enhanced logging middleware with all required fields
app.UseMiddleware<RequestLoggingMiddleware>();

// Map reverse proxy routes (this forwards to backend API)
app.MapReverseProxy();

app.Logger.LogInformation("API Gateway started successfully");
app.Logger.LogInformation("Rate limiting enabled at Gateway level");
app.Logger.LogInformation("Comprehensive request/response logging enabled");

app.Run();
