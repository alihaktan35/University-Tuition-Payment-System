namespace UniversityTuitionAPI.Models;

public class Payment
{
    public int Id { get; set; }
    public int TuitionId { get; set; }
    public int StudentId { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // "Successful", "Error", "Partial"
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public Tuition? Tuition { get; set; }
    public Student? Student { get; set; }
}
