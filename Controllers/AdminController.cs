using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityTuitionAPI.DTOs;
using UniversityTuitionAPI.Services;

namespace UniversityTuitionAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Add tuition for a single student
    /// Requires Admin authentication
    /// </summary>
    /// <param name="request">Tuition details</param>
    /// <returns>Transaction status</returns>
    [HttpPost("tuition")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> AddTuition([FromBody] AddTuitionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StudentNo) || string.IsNullOrWhiteSpace(request.Term))
        {
            return BadRequest(new { message = "StudentNo and Term are required" });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero" });
        }

        var result = await _adminService.AddTuitionAsync(request);

        if (result.Status == "Error")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add tuitions from CSV file
    /// Requires Admin authentication
    /// CSV Format: StudentNo,Term,Amount
    /// </summary>
    /// <param name="file">CSV file</param>
    /// <returns>Transaction status with number of records processed</returns>
    [HttpPost("tuition/batch")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> AddTuitionBatch(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "CSV file is required" });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "File must be a CSV file" });
        }

        using var stream = file.OpenReadStream();
        var result = await _adminService.AddTuitionBatchAsync(stream);

        return Ok(result);
    }

    /// <summary>
    /// Get list of students with unpaid tuition for a specific term
    /// Requires Admin authentication
    /// Supports paging
    /// </summary>
    /// <param name="term">Term (e.g., 2024-Fall)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>List of students with unpaid tuition</returns>
    [HttpGet("tuition/unpaid/{term}")]
    [ProducesResponseType(typeof(UnpaidTuitionResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetUnpaidTuitions(
        string term,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1 || pageSize > 100)
            pageSize = 10;

        var result = await _adminService.GetUnpaidTuitionsAsync(term, pageNumber, pageSize);

        return Ok(result);
    }
}
