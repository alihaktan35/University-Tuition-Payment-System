namespace UniversityTuitionAPI.DTOs;

public class TuitionQueryResponse
{
    public string StudentNo { get; set; } = string.Empty;
    public decimal TuitionTotal { get; set; }
    public decimal Balance { get; set; }
}
