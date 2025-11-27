using UniversityTuitionAPI.DTOs;

namespace UniversityTuitionAPI.Services;

public interface ITuitionService
{
    Task<TuitionQueryResponse?> GetTuitionInfoAsync(string studentNo);
    Task<List<TuitionQueryResponse>> GetAllTuitionsForStudentAsync(string studentNo);
}
