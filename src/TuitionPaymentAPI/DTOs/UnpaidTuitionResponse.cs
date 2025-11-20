namespace TuitionPaymentAPI.DTOs;

public class UnpaidTuitionResponse
{
    public List<UnpaidStudent> Students { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class UnpaidStudent
{
    public string StudentNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal TuitionTotal { get; set; }
    public decimal Balance { get; set; }
}

public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
