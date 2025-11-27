using Microsoft.AspNetCore.Mvc;
using UniversityTuitionAPI.DTOs;
using UniversityTuitionAPI.Services;

namespace UniversityTuitionAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mobile")]
public class MobileAppController : ControllerBase
{
    private readonly ITuitionService _tuitionService;
    private readonly IRateLimitService _rateLimitService;

    public MobileAppController(ITuitionService tuitionService, IRateLimitService rateLimitService)
    {
        _tuitionService = tuitionService;
        _rateLimitService = rateLimitService;
    }

    /// <summary>
    /// Query tuition information for a student (Mobile App)
    /// Limited to 3 calls per student per day
    /// </summary>
    /// <param name="studentNo">Student number</param>
    /// <returns>Tuition total and balance</returns>
    [HttpGet("tuition/{studentNo}")]
    [ProducesResponseType(typeof(TuitionQueryResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> QueryTuition(string studentNo)
    {
        // Check rate limit (3 per student per day)
        var endpoint = "mobile/tuition";
        if (await _rateLimitService.IsRateLimitExceededAsync(studentNo, endpoint, 3))
        {
            return StatusCode(429, new
            {
                message = "Rate limit exceeded. Maximum 3 requests per day allowed.",
                error = "TooManyRequests"
            });
        }

        var result = await _tuitionService.GetTuitionInfoAsync(studentNo);

        if (result == null)
        {
            return NotFound(new { message = $"No tuition information found for student {studentNo}" });
        }

        // Record the request
        await _rateLimitService.RecordRequestAsync(studentNo, endpoint);

        return Ok(result);
    }
}
