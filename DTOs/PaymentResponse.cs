namespace UniversityTuitionAPI.DTOs;

public class PaymentResponse
{
    public string Status { get; set; } = string.Empty; // "Successful", "Error", "Partial"
    public decimal AmountPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public string? Message { get; set; }
}
