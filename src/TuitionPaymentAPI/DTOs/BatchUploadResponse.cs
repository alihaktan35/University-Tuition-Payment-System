namespace TuitionPaymentAPI.DTOs;

public class BatchUploadResponse
{
    public string Status { get; set; } = string.Empty;
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<BatchErrorDetail> Errors { get; set; } = new();
}

public class BatchErrorDetail
{
    public int Row { get; set; }
    public string Reason { get; set; } = string.Empty;
}
