namespace UniversityTuitionAPI.Models;

public class Tuition
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty; // e.g., "2024-Fall", "2025-Spring"
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }

    // Navigation properties
    public Student? Student { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
