using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using TuitionPaymentAPI.Data;
using TuitionPaymentAPI.DTOs;
using TuitionPaymentAPI.Models;

namespace TuitionPaymentAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly TuitionDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(TuitionDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Add tuition for a single student
    /// </summary>
    /// <param name="request">Tuition details</param>
    /// <returns>Success status and tuition ID</returns>
    [HttpPost("tuition")]
    [ProducesResponseType(typeof(AddTuitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTuition([FromBody] AddTuitionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation($"Adding tuition for student: {request.StudentNo}, Term: {request.Term}, Amount: {request.Amount}");

        // Find student
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentNo == request.StudentNo);

        if (student == null)
        {
            return NotFound(new AddTuitionResponse
            {
                Status = "Error",
                Message = $"Student with number {request.StudentNo} not found"
            });
        }

        // Check if tuition already exists for this student and term
        var existingTuition = await _context.Tuitions
            .FirstOrDefaultAsync(t => t.StudentId == student.StudentId && t.Term == request.Term);

        if (existingTuition != null)
        {
            // Update existing tuition
            existingTuition.TotalAmount = request.Amount;
            existingTuition.Balance = request.Amount - existingTuition.PaidAmount;
            existingTuition.UpdatedAt = DateTime.UtcNow;

            // Update status
            if (existingTuition.Balance == 0)
            {
                existingTuition.Status = "PAID";
            }
            else if (existingTuition.PaidAmount > 0)
            {
                existingTuition.Status = "PARTIAL";
            }
            else
            {
                existingTuition.Status = "UNPAID";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Tuition updated for student {request.StudentNo}. Tuition ID: {existingTuition.TuitionId}");

            return Ok(new AddTuitionResponse
            {
                Status = "Updated",
                TuitionId = existingTuition.TuitionId,
                Message = "Tuition record updated successfully"
            });
        }

        // Create new tuition
        var tuition = new Tuition
        {
            StudentId = student.StudentId,
            Term = request.Term,
            TotalAmount = request.Amount,
            Balance = request.Amount,
            PaidAmount = 0,
            Status = "UNPAID",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tuitions.Add(tuition);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Tuition created for student {request.StudentNo}. Tuition ID: {tuition.TuitionId}");

        return CreatedAtAction(nameof(AddTuition), new AddTuitionResponse
        {
            Status = "Success",
            TuitionId = tuition.TuitionId,
            Message = "Tuition record created successfully"
        });
    }

    /// <summary>
    /// Add tuition for multiple students via CSV upload
    /// </summary>
    /// <param name="file">CSV file with columns: studentNo, term, amount</param>
    /// <returns>Summary of successful and failed imports</returns>
    [HttpPost("tuition/batch")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BatchUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BatchAddTuition(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new BatchUploadResponse
            {
                Status = "Error",
                ErrorCount = 1,
                Errors = new List<BatchErrorDetail> { new BatchErrorDetail { Row = 0, Reason = "No file uploaded" } }
            });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new BatchUploadResponse
            {
                Status = "Error",
                ErrorCount = 1,
                Errors = new List<BatchErrorDetail> { new BatchErrorDetail { Row = 0, Reason = "File must be a CSV file" } }
            });
        }

        if (file.Length > 10 * 1024 * 1024) // 10 MB limit
        {
            return BadRequest(new BatchUploadResponse
            {
                Status = "Error",
                ErrorCount = 1,
                Errors = new List<BatchErrorDetail> { new BatchErrorDetail { Row = 0, Reason = "File size exceeds 10 MB limit" } }
            });
        }

        _logger.LogInformation($"Processing batch upload: {file.FileName}");

        var response = new BatchUploadResponse { Status = "Success" };
        var errors = new List<BatchErrorDetail>();
        int rowNumber = 0;
        int successCount = 0;

        try
        {
            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            });

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                rowNumber++;

                try
                {
                    var studentNo = csv.GetField<string>("studentNo");
                    var term = csv.GetField<string>("term");
                    var amountStr = csv.GetField<string>("amount");

                    // Validate row data
                    if (string.IsNullOrWhiteSpace(studentNo))
                    {
                        errors.Add(new BatchErrorDetail { Row = rowNumber, Reason = "Student number is required" });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(term))
                    {
                        errors.Add(new BatchErrorDetail { Row = rowNumber, Reason = "Term is required" });
                        continue;
                    }

                    if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
                    {
                        errors.Add(new BatchErrorDetail { Row = rowNumber, Reason = "Amount must be a positive number" });
                        continue;
                    }

                    // Find student
                    var student = await _context.Students
                        .FirstOrDefaultAsync(s => s.StudentNo == studentNo);

                    if (student == null)
                    {
                        errors.Add(new BatchErrorDetail { Row = rowNumber, Reason = $"Student {studentNo} not found" });
                        continue;
                    }

                    // Check if tuition exists
                    var existingTuition = await _context.Tuitions
                        .FirstOrDefaultAsync(t => t.StudentId == student.StudentId && t.Term == term);

                    if (existingTuition != null)
                    {
                        // Update existing
                        existingTuition.TotalAmount = amount;
                        existingTuition.Balance = amount - existingTuition.PaidAmount;
                        existingTuition.UpdatedAt = DateTime.UtcNow;

                        if (existingTuition.Balance == 0)
                            existingTuition.Status = "PAID";
                        else if (existingTuition.PaidAmount > 0)
                            existingTuition.Status = "PARTIAL";
                        else
                            existingTuition.Status = "UNPAID";
                    }
                    else
                    {
                        // Create new
                        var tuition = new Tuition
                        {
                            StudentId = student.StudentId,
                            Term = term,
                            TotalAmount = amount,
                            Balance = amount,
                            PaidAmount = 0,
                            Status = "UNPAID",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Tuitions.Add(tuition);
                    }

                    await _context.SaveChangesAsync();
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing row {rowNumber}");
                    errors.Add(new BatchErrorDetail { Row = rowNumber, Reason = $"Processing error: {ex.Message}" });
                }
            }

            response.SuccessCount = successCount;
            response.ErrorCount = errors.Count;
            response.Errors = errors;

            if (errors.Count > 0)
            {
                response.Status = "Partial Success";
            }

            _logger.LogInformation($"Batch upload completed. Success: {successCount}, Errors: {errors.Count}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CSV file");
            return StatusCode(500, new BatchUploadResponse
            {
                Status = "Error",
                ErrorCount = 1,
                Errors = new List<BatchErrorDetail> { new BatchErrorDetail { Row = 0, Reason = $"File processing error: {ex.Message}" } }
            });
        }
    }

    /// <summary>
    /// Get list of students with unpaid tuition for a given term
    /// </summary>
    /// <param name="term">Academic term (e.g., 2024-Fall)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of students with unpaid tuition</returns>
    [HttpGet("unpaid/{term}")]
    [ProducesResponseType(typeof(UnpaidTuitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnpaidTuition(
        string term,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1)
        {
            return BadRequest(new { error = new { code = "INVALID_PAGE", message = "Page number must be greater than 0" } });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = new { code = "INVALID_PAGE_SIZE", message = "Page size must be between 1 and 100" } });
        }

        _logger.LogInformation($"Querying unpaid tuition for term: {term}, page: {page}, pageSize: {pageSize}");

        // Query students with unpaid or partial tuition
        var query = _context.Tuitions
            .Include(t => t.Student)
            .Where(t => t.Term == term && (t.Status == "UNPAID" || t.Status == "PARTIAL"))
            .OrderBy(t => t.Student.StudentNo);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var tuitions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var students = tuitions.Select(t => new UnpaidStudent
        {
            StudentNo = t.Student.StudentNo,
            Name = t.Student.Name,
            TuitionTotal = t.TotalAmount,
            Balance = t.Balance
        }).ToList();

        var response = new UnpaidTuitionResponse
        {
            Students = students,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            }
        };

        return Ok(response);
    }
}
