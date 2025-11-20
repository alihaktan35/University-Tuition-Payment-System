using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuitionPaymentAPI.Data;
using TuitionPaymentAPI.DTOs;
using TuitionPaymentAPI.Models;

namespace TuitionPaymentAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class BankingController : ControllerBase
{
    private readonly TuitionDbContext _context;
    private readonly ILogger<BankingController> _logger;

    public BankingController(TuitionDbContext context, ILogger<BankingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Query tuition amount and balance for a student (Banking App Endpoint)
    /// Requires authentication
    /// </summary>
    /// <param name="studentNo">Student number</param>
    /// <returns>Tuition total and balance</returns>
    [HttpGet("tuition/{studentNo}")]
    [Authorize]
    [ProducesResponseType(typeof(TuitionQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QueryTuition(string studentNo)
    {
        _logger.LogInformation($"Banking: Querying tuition for student: {studentNo}");

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

    /// <summary>
    /// Process a tuition payment (supports partial payments)
    /// </summary>
    /// <param name="request">Payment details</param>
    /// <returns>Payment status and remaining balance</returns>
    [HttpPost("pay")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PayTuition([FromBody] PaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation($"Processing payment for student: {request.StudentNo}, Term: {request.Term}, Amount: {request.Amount}");

        // Find student
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentNo == request.StudentNo);

        if (student == null)
        {
            return NotFound(new PaymentResponse
            {
                Status = "Error",
                RemainingBalance = 0,
                Message = $"Student with number {request.StudentNo} not found"
            });
        }

        // Find tuition record for the term
        var tuition = await _context.Tuitions
            .FirstOrDefaultAsync(t => t.StudentId == student.StudentId && t.Term == request.Term);

        if (tuition == null)
        {
            return NotFound(new PaymentResponse
            {
                Status = "Error",
                RemainingBalance = 0,
                Message = $"No tuition record found for student {request.StudentNo} and term {request.Term}"
            });
        }

        // Validate payment amount
        if (request.Amount <= 0)
        {
            return BadRequest(new PaymentResponse
            {
                Status = "Error",
                RemainingBalance = tuition.Balance,
                Message = "Payment amount must be greater than 0"
            });
        }

        if (request.Amount > tuition.Balance)
        {
            return BadRequest(new PaymentResponse
            {
                Status = "Error",
                RemainingBalance = tuition.Balance,
                Message = $"Payment amount ({request.Amount}) exceeds remaining balance ({tuition.Balance})"
            });
        }

        // Process payment
        try
        {
            tuition.PaidAmount += request.Amount;
            tuition.Balance -= request.Amount;
            tuition.UpdatedAt = DateTime.UtcNow;

            // Update status
            if (tuition.Balance == 0)
            {
                tuition.Status = "PAID";
            }
            else if (tuition.PaidAmount > 0 && tuition.Balance > 0)
            {
                tuition.Status = "PARTIAL";
            }

            // Create payment record
            var payment = new Payment
            {
                TuitionId = tuition.TuitionId,
                Amount = request.Amount,
                PaymentDate = DateTime.UtcNow,
                Status = "Successful",
                TransactionReference = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper()
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Payment processed successfully. Transaction Ref: {payment.TransactionReference}, Remaining Balance: {tuition.Balance}");

            return Ok(new PaymentResponse
            {
                Status = "Successful",
                RemainingBalance = tuition.Balance,
                Message = tuition.Balance == 0 ? "Payment completed. Tuition fully paid." : $"Partial payment processed. Remaining balance: {tuition.Balance:C}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return StatusCode(500, new PaymentResponse
            {
                Status = "Error",
                RemainingBalance = tuition.Balance,
                Message = "An error occurred while processing the payment"
            });
        }
    }
}
