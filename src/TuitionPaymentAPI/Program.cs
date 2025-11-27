using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TuitionPaymentAPI.Data;
using TuitionPaymentAPI.Middleware;
using TuitionPaymentAPI.Models;
using TuitionPaymentAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register services
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure Entity Framework - SQLite for Development, SQL Server for Production
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (builder.Environment.IsProduction() && connectionString!.Contains("database.windows.net"))
{
    // Use SQL Server for Azure production
    builder.Services.AddDbContext<TuitionDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Use SQLite for local development
    builder.Services.AddDbContext<TuitionDbContext>(options =>
        options.UseSqlite(connectionString));
}

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure Swagger/OpenAPI with JWT Bearer authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "University Tuition Payment API",
        Version = "v1",
        Description = "API for managing university tuition payments with mobile app, banking, and admin interfaces"
    });

    // Configure Swagger to use API Gateway URLs
    // IMPORTANT: All API requests MUST go through the API Gateway
    options.AddServer(new OpenApiServer
    {
        Url = "http://localhost:5000",
        Description = "API Gateway (Local Development)"
    });
    options.AddServer(new OpenApiServer
    {
        Url = "https://ahs-tuition-gateway.azurewebsites.net",
        Description = "API Gateway (Azure Production)"
    });

    // Add JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

// Configure the HTTP request pipeline
// Enable Swagger in all environments for this project
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "University Tuition Payment API v1");
    options.RoutePrefix = "swagger";
});

// Disable HTTPS redirection in production (Azure handles this at load balancer level)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// NOTE: Rate limiting has been moved to API Gateway level
// Logging is also handled at Gateway level for comprehensive request/response tracking

app.MapControllers();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TuitionDbContext>();
        context.Database.Migrate();

        // Seed users programmatically if they don't exist
        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "bankapi",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Bank123!"),
                    Role = "BankingSystem",
                    CreatedAt = DateTime.UtcNow
                }
            );
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();
