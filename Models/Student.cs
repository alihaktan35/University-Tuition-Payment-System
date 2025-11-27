namespace UniversityTuitionAPI.Models;

public class Student
{
    public int Id { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Tuition> Tuitions { get; set; } = new List<Tuition>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
