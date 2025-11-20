namespace TuitionPaymentAPI.DTOs;

public class AddTuitionResponse
{
    public string Status { get; set; } = string.Empty;
    public int? TuitionId { get; set; }
    public string? Message { get; set; }
}
