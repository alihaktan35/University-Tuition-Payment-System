using System.ComponentModel.DataAnnotations;

namespace TuitionPaymentAPI.DTOs;

public class PaymentRequest
{
    [Required]
    public string StudentNo { get; set; } = string.Empty;

    [Required]
    public string Term { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
}
