using Microsoft.EntityFrameworkCore;
using UniversityTuitionAPI.Data;
using UniversityTuitionAPI.Models;

namespace UniversityTuitionAPI.Services;

public class RateLimitService : IRateLimitService
{
    private readonly ApplicationDbContext _context;

    public RateLimitService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsRateLimitExceededAsync(string studentNo, string endpoint, int maxRequests = 3)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var requestCount = await _context.RateLimits
            .Where(r => r.StudentNo == studentNo
                     && r.Endpoint == endpoint
                     && r.RequestDate >= today
                     && r.RequestDate < tomorrow)
            .SumAsync(r => r.RequestCount);

        return requestCount >= maxRequests;
    }

    public async Task RecordRequestAsync(string studentNo, string endpoint)
    {
        var today = DateTime.UtcNow.Date;

        var rateLimit = await _context.RateLimits
            .FirstOrDefaultAsync(r => r.StudentNo == studentNo
                                   && r.Endpoint == endpoint
                                   && r.RequestDate == today);

        if (rateLimit == null)
        {
            rateLimit = new RateLimit
            {
                StudentNo = studentNo,
                Endpoint = endpoint,
                RequestDate = today,
                RequestCount = 1
            };
            _context.RateLimits.Add(rateLimit);
        }
        else
        {
            rateLimit.RequestCount++;
            _context.RateLimits.Update(rateLimit);
        }

        await _context.SaveChangesAsync();
    }
}
