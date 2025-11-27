namespace UniversityTuitionAPI.Models;

public class ApiLog
{
    public int Id { get; set; }

    // Request-level logs
    public string HttpMethod { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public DateTime RequestTimestamp { get; set; }
    public string SourceIpAddress { get; set; } = string.Empty;
    public string? RequestHeaders { get; set; }
    public long RequestSize { get; set; }
    public bool AuthenticationSucceeded { get; set; }

    // Response-level logs
    public int StatusCode { get; set; }
    public long ResponseLatencyMs { get; set; }
    public long ResponseSize { get; set; }
    public string? ErrorMessage { get; set; }
}
