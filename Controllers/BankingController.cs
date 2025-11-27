using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityTuitionAPI.DTOs;
using UniversityTuitionAPI.Services;

namespace UniversityTuitionAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/banking")]
public class BankingController : ControllerBase
{
    private readonly ITuitionService _tuitionService;
    private readonly IPaymentService _paymentService;

    public BankingController(ITuitionService tuitionService, IPaymentService paymentService)
    {
        _tuitionService = tuitionService;
        _paymentService = paymentService;
    }

    /// <summary>
    /// Query tuition information for a student (Banking App)
    /// Requires authentication
    /// </summary>
    /// <param name="studentNo">Student number</param>
    /// <returns>Tuition total and balance</returns>
    [HttpGet("tuition/{studentNo}")]
    [Authorize(Roles = "Banking,Admin")]
    [ProducesResponseType(typeof(TuitionQueryResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> QueryTuition(string studentNo)
    {
        var result = await _tuitionService.GetTuitionInfoAsync(studentNo);

        if (result == null)
        {
            return NotFound(new { message = $"No tuition information found for student {studentNo}" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Process tuition payment for a student
    /// No authentication required
    /// </summary>
    /// <param name="request">Payment details</param>
    /// <returns>Payment status</returns>
    [HttpPost("payment")]
    [ProducesResponseType(typeof(PaymentResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> PayTuition([FromBody] PaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StudentNo) || string.IsNullOrWhiteSpace(request.Term))
        {
            return BadRequest(new { message = "StudentNo and Term are required" });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Payment amount must be greater than zero" });
        }

        var result = await _paymentService.ProcessPaymentAsync(request);

        if (result.Status == "Error")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
