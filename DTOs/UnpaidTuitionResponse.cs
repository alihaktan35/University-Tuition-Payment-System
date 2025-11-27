namespace UniversityTuitionAPI.DTOs;

public class UnpaidTuitionResponse
{
    public string Term { get; set; } = string.Empty;
    public List<UnpaidStudent> Students { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UnpaidStudent
{
    public string StudentNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal TuitionAmount { get; set; }
    public decimal Balance { get; set; }
}
