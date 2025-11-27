using UniversityTuitionAPI.DTOs;

namespace UniversityTuitionAPI.Services;

public interface IAdminService
{
    Task<TransactionResponse> AddTuitionAsync(AddTuitionRequest request);
    Task<TransactionResponse> AddTuitionBatchAsync(Stream csvStream);
    Task<UnpaidTuitionResponse> GetUnpaidTuitionsAsync(string term, int pageNumber = 1, int pageSize = 10);
}
