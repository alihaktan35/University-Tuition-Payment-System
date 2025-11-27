using UniversityTuitionAPI.Models;

namespace UniversityTuitionAPI.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Check if database has data
        if (context.Students.Any())
        {
            return; // DB has been seeded
        }

        // Seed Users
        var users = new[]
        {
            new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin"
            },
            new User
            {
                Username = "banking",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("banking123"),
                Role = "Banking"
            }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        // Seed Students
        var students = new[]
        {
            new Student { StudentNo = "2021001", Name = "Ahmet Yılmaz", Email = "ahmet@university.edu" },
            new Student { StudentNo = "2021002", Name = "Ayşe Kaya", Email = "ayse@university.edu" },
            new Student { StudentNo = "2021003", Name = "Mehmet Demir", Email = "mehmet@university.edu" },
            new Student { StudentNo = "2021004", Name = "Fatma Şahin", Email = "fatma@university.edu" },
            new Student { StudentNo = "2021005", Name = "Ali Çelik", Email = "ali@university.edu" }
        };
        context.Students.AddRange(students);
        context.SaveChanges();

        // Seed Tuitions
        var tuitions = new List<Tuition>();
        foreach (var student in students)
        {
            // Add tuition for Fall 2024
            tuitions.Add(new Tuition
            {
                StudentId = student.Id,
                StudentNo = student.StudentNo,
                Term = "2024-Fall",
                Amount = 15000.00m,
                Balance = 15000.00m,
                IsPaid = false
            });

            // Add tuition for Spring 2025
            tuitions.Add(new Tuition
            {
                StudentId = student.Id,
                StudentNo = student.StudentNo,
                Term = "2025-Spring",
                Amount = 15000.00m,
                Balance = 15000.00m,
                IsPaid = false
            });
        }

        // Make some tuitions paid or partially paid
        tuitions[0].Balance = 0;
        tuitions[0].IsPaid = true;
        tuitions[0].PaidAt = DateTime.UtcNow.AddDays(-10);

        tuitions[2].Balance = 7500.00m;

        context.Tuitions.AddRange(tuitions);
        context.SaveChanges();

        // Seed some payments
        var payments = new[]
        {
            new Payment
            {
                TuitionId = tuitions[0].Id,
                StudentId = tuitions[0].StudentId,
                StudentNo = tuitions[0].StudentNo,
                Term = tuitions[0].Term,
                Amount = 15000.00m,
                Status = "Successful",
                PaymentDate = DateTime.UtcNow.AddDays(-10)
            },
            new Payment
            {
                TuitionId = tuitions[2].Id,
                StudentId = tuitions[2].StudentId,
                StudentNo = tuitions[2].StudentNo,
                Term = tuitions[2].Term,
                Amount = 7500.00m,
                Status = "Partial",
                PaymentDate = DateTime.UtcNow.AddDays(-5)
            }
        };
        context.Payments.AddRange(payments);
        context.SaveChanges();
    }
}
