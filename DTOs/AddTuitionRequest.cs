namespace UniversityTuitionAPI.DTOs;

public class AddTuitionRequest
{
    public string StudentNo { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
