namespace TuitionPaymentAPI.DTOs;

public class PaymentResponse
{
    public string Status { get; set; } = string.Empty; // Successful, Error
    public decimal RemainingBalance { get; set; }
    public string? Message { get; set; }
}
