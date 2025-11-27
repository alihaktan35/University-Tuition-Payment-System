using Microsoft.EntityFrameworkCore;
using UniversityTuitionAPI.Models;

namespace UniversityTuitionAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Tuition> Tuitions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<RateLimit> RateLimits { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ApiLog> ApiLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Student configuration
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StudentNo).IsUnique();
            entity.Property(e => e.StudentNo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
        });

        // Tuition configuration
        modelBuilder.Entity<Tuition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.StudentNo, e.Term }).IsUnique();
            entity.Property(e => e.StudentNo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Term).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Tuitions)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StudentNo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Term).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Tuition)
                .WithMany(t => t.Payments)
                .HasForeignKey(e => e.TuitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Payments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // RateLimit configuration
        modelBuilder.Entity<RateLimit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.StudentNo, e.Endpoint, e.RequestDate });
            entity.Property(e => e.StudentNo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Endpoint).IsRequired().HasMaxLength(200);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });

        // ApiLog configuration
        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RequestTimestamp);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.RequestPath).HasMaxLength(500);
            entity.Property(e => e.SourceIpAddress).HasMaxLength(50);
        });
    }
}
