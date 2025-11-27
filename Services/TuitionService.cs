using Microsoft.EntityFrameworkCore;
using UniversityTuitionAPI.Data;
using UniversityTuitionAPI.DTOs;

namespace UniversityTuitionAPI.Services;

public class TuitionService : ITuitionService
{
    private readonly ApplicationDbContext _context;

    public TuitionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TuitionQueryResponse?> GetTuitionInfoAsync(string studentNo)
    {
        var tuitions = await _context.Tuitions
            .Where(t => t.StudentNo == studentNo)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        if (!tuitions.Any())
        {
            return null;
        }

        var totalTuition = tuitions.Sum(t => t.Amount);
        var totalBalance = tuitions.Sum(t => t.Balance);

        return new TuitionQueryResponse
        {
            StudentNo = studentNo,
            TuitionTotal = totalTuition,
            Balance = totalBalance
        };
    }

    public async Task<List<TuitionQueryResponse>> GetAllTuitionsForStudentAsync(string studentNo)
    {
        var tuitions = await _context.Tuitions
            .Where(t => t.StudentNo == studentNo)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        // Note: This method aggregates all tuitions into a single response
        // matching the simplified TuitionQueryResponse structure
        var totalTuition = tuitions.Sum(t => t.Amount);
        var totalBalance = tuitions.Sum(t => t.Balance);

        if (!tuitions.Any())
        {
            return new List<TuitionQueryResponse>();
        }

        return new List<TuitionQueryResponse>
        {
            new TuitionQueryResponse
            {
                StudentNo = studentNo,
                TuitionTotal = totalTuition,
                Balance = totalBalance
            }
        };
    }
}
