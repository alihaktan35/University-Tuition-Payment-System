using Microsoft.EntityFrameworkCore;
using TuitionPaymentAPI.Models;

namespace TuitionPaymentAPI.Data;

public class TuitionDbContext : DbContext
{
    public TuitionDbContext(DbContextOptions<TuitionDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Suppress the pending model changes warning for production deployment
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Tuition> Tuitions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<RateLimit> RateLimits { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Student entity
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.StudentNo).IsUnique();
            entity.HasMany(e => e.Tuitions)
                  .WithOne(e => e.Student)
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Tuition entity
        modelBuilder.Entity<Tuition>(entity =>
        {
            entity.HasMany(e => e.Payments)
                  .WithOne(e => e.Tuition)
                  .HasForeignKey(e => e.TuitionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.StudentId, e.Term });
        });

        // Configure RateLimit entity
        modelBuilder.Entity<RateLimit>(entity =>
        {
            entity.HasIndex(e => new { e.StudentNo, e.Endpoint, e.Date });
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Use fixed datetime for consistent seed data across migrations
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed Students
        modelBuilder.Entity<Student>().HasData(
            new Student { StudentId = 1, StudentNo = "20210001", Name = "Ahmet Yılmaz", Email = "ahmet.yilmaz@university.edu", CreatedAt = seedDate },
            new Student { StudentId = 2, StudentNo = "20210002", Name = "Ayşe Demir", Email = "ayse.demir@university.edu", CreatedAt = seedDate },
            new Student { StudentId = 3, StudentNo = "20210003", Name = "Mehmet Kaya", Email = "mehmet.kaya@university.edu", CreatedAt = seedDate },
            new Student { StudentId = 4, StudentNo = "20210004", Name = "Fatma Öz", Email = "fatma.oz@university.edu", CreatedAt = seedDate },
            new Student { StudentId = 5, StudentNo = "20210005", Name = "Ali Şahin", Email = "ali.sahin@university.edu", CreatedAt = seedDate }
        );

        // Seed Tuitions
        modelBuilder.Entity<Tuition>().HasData(
            new Tuition { TuitionId = 1, StudentId = 1, Term = "2024-Fall", TotalAmount = 50000, Balance = 50000, PaidAmount = 0, Status = "UNPAID", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Tuition { TuitionId = 2, StudentId = 2, Term = "2024-Fall", TotalAmount = 50000, Balance = 25000, PaidAmount = 25000, Status = "PARTIAL", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Tuition { TuitionId = 3, StudentId = 3, Term = "2024-Fall", TotalAmount = 50000, Balance = 0, PaidAmount = 50000, Status = "PAID", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Tuition { TuitionId = 4, StudentId = 4, Term = "2024-Fall", TotalAmount = 50000, Balance = 50000, PaidAmount = 0, Status = "UNPAID", CreatedAt = seedDate, UpdatedAt = seedDate }
        );

        // Note: User seeding removed due to BCrypt non-deterministic hashing
        // Users will be created programmatically on first application start
        // See Program.cs for user initialization logic
    }
}
