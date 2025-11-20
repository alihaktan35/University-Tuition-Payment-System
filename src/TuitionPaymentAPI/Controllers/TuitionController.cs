using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuitionPaymentAPI.Data;
using TuitionPaymentAPI.DTOs;

namespace TuitionPaymentAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class TuitionController : ControllerBase
{
    private readonly TuitionDbContext _context;
    private readonly ILogger<TuitionController> _logger;

    public TuitionController(TuitionDbContext context, ILogger<TuitionController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Query tuition amount and balance for a student (Mobile App Endpoint)
    /// Rate limited to 3 requests per student per day
    /// </summary>
    /// <param name="studentNo">Student number</param>
    /// <returns>Tuition total and balance</returns>
    [HttpGet("query/{studentNo}")]
    [ProducesResponseType(typeof(TuitionQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> QueryTuition(string studentNo)
    {
        _logger.LogInformation($"Querying tuition for student: {studentNo}");

        var student = await _context.Students
            .Include(s => s.Tuitions)
            .FirstOrDefaultAsync(s => s.StudentNo == studentNo);

        if (student == null)
        {
            return NotFound(new
            {
                error = new
                {
                    code = "STUDENT_NOT_FOUND",
                    message = $"Student with number {studentNo} not found"
                }
            });
        }

        // Get the most recent tuition record
        var tuition = student.Tuitions.OrderByDescending(t => t.CreatedAt).FirstOrDefault();

        if (tuition == null)
        {
            return NotFound(new
            {
                error = new
                {
                    code = "TUITION_NOT_FOUND",
                    message = $"No tuition records found for student {studentNo}"
                }
            });
        }

        return Ok(new TuitionQueryResponse
        {
            TuitionTotal = tuition.TotalAmount,
            Balance = tuition.Balance
        });
    }
}
