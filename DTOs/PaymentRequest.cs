namespace UniversityTuitionAPI.DTOs;

public class PaymentRequest
{
    public string StudentNo { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
