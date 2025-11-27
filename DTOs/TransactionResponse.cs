namespace UniversityTuitionAPI.DTOs;

public class TransactionResponse
{
    public string Status { get; set; } = string.Empty; // "Success", "Error"
    public string? Message { get; set; }
    public int? RecordsProcessed { get; set; }
}
