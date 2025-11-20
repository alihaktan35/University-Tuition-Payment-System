using Microsoft.EntityFrameworkCore;
using TuitionPaymentAPI.Models;

namespace TuitionPaymentAPI.Data;

public class TuitionDbContext : DbContext
{
    public TuitionDbContext(DbContextOptions<TuitionDbContext> options) : base(options)
    {
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
        // Seed Students
        modelBuilder.Entity<Student>().HasData(
            new Student { StudentId = 1, StudentNo = "20210001", Name = "Ahmet Yılmaz", Email = "ahmet.yilmaz@university.edu", CreatedAt = DateTime.UtcNow },
            new Student { StudentId = 2, StudentNo = "20210002", Name = "Ayşe Demir", Email = "ayse.demir@university.edu", CreatedAt = DateTime.UtcNow },
            new Student { StudentId = 3, StudentNo = "20210003", Name = "Mehmet Kaya", Email = "mehmet.kaya@university.edu", CreatedAt = DateTime.UtcNow },
            new Student { StudentId = 4, StudentNo = "20210004", Name = "Fatma Öz", Email = "fatma.oz@university.edu", CreatedAt = DateTime.UtcNow },
            new Student { StudentId = 5, StudentNo = "20210005", Name = "Ali Şahin", Email = "ali.sahin@university.edu", CreatedAt = DateTime.UtcNow }
        );

        // Seed Tuitions
        modelBuilder.Entity<Tuition>().HasData(
            new Tuition { TuitionId = 1, StudentId = 1, Term = "2024-Fall", TotalAmount = 50000, Balance = 50000, PaidAmount = 0, Status = "UNPAID", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Tuition { TuitionId = 2, StudentId = 2, Term = "2024-Fall", TotalAmount = 50000, Balance = 25000, PaidAmount = 25000, Status = "PARTIAL", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Tuition { TuitionId = 3, StudentId = 3, Term = "2024-Fall", TotalAmount = 50000, Balance = 0, PaidAmount = 50000, Status = "PAID", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Tuition { TuitionId = 4, StudentId = 4, Term = "2024-Fall", TotalAmount = 50000, Balance = 50000, PaidAmount = 0, Status = "UNPAID", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        // Seed Users (password: Admin123! and Bank123!)
        // Using BCrypt for password hashing
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 2,
                Username = "bankapi",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Bank123!"),
                Role = "BankingSystem",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
