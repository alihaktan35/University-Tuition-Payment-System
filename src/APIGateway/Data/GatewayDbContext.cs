using Microsoft.EntityFrameworkCore;
using APIGateway.Models;

namespace APIGateway.Data;

public class GatewayDbContext : DbContext
{
    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Suppress the pending model changes warning for production deployment
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<RateLimit> RateLimits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure RateLimit entity
        modelBuilder.Entity<RateLimit>(entity =>
        {
            entity.ToTable("RateLimits");
            entity.HasKey(e => e.RateLimitId);

            // Create composite index for efficient queries
            entity.HasIndex(e => new { e.StudentNo, e.Endpoint, e.Date })
                  .HasDatabaseName("IX_RateLimit_StudentNo_Endpoint_Date");
        });
    }
}
