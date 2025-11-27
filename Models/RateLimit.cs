namespace UniversityTuitionAPI.Models;

public class RateLimit
{
    public int Id { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public int RequestCount { get; set; }
}
