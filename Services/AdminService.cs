using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using UniversityTuitionAPI.Data;
using UniversityTuitionAPI.DTOs;
using UniversityTuitionAPI.Models;

namespace UniversityTuitionAPI.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionResponse> AddTuitionAsync(AddTuitionRequest request)
    {
        try
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNo == request.StudentNo);

            if (student == null)
            {
                // Create new student
                student = new Student
                {
                    StudentNo = request.StudentNo,
                    Name = $"Student {request.StudentNo}",
                    Email = $"{request.StudentNo}@university.edu"
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }

            // Check if tuition already exists
            var existingTuition = await _context.Tuitions
                .FirstOrDefaultAsync(t => t.StudentNo == request.StudentNo && t.Term == request.Term);

            if (existingTuition != null)
            {
                return new TransactionResponse
                {
                    Status = "Error",
                    Message = $"Tuition already exists for student {request.StudentNo} in term {request.Term}"
                };
            }

            var tuition = new Tuition
            {
                StudentId = student.Id,
                StudentNo = request.StudentNo,
                Term = request.Term,
                Amount = request.Amount,
                Balance = request.Amount,
                IsPaid = false
            };

            _context.Tuitions.Add(tuition);
            await _context.SaveChangesAsync();

            return new TransactionResponse
            {
                Status = "Success",
                Message = $"Tuition added successfully for student {request.StudentNo}",
                RecordsProcessed = 1
            };
        }
        catch (Exception ex)
        {
            return new TransactionResponse
            {
                Status = "Error",
                Message = $"Error adding tuition: {ex.Message}"
            };
        }
    }

    public async Task<TransactionResponse> AddTuitionBatchAsync(Stream csvStream)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<TuitionCsvRecord>().ToList();
            var processedCount = 0;
            var errors = new List<string>();

            foreach (var record in records)
            {
                try
                {
                    var student = await _context.Students
                        .FirstOrDefaultAsync(s => s.StudentNo == record.StudentNo);

                    if (student == null)
                    {
                        student = new Student
                        {
                            StudentNo = record.StudentNo,
                            Name = $"Student {record.StudentNo}",
                            Email = $"{record.StudentNo}@university.edu"
                        };
                        _context.Students.Add(student);
                        await _context.SaveChangesAsync();
                    }

                    var existingTuition = await _context.Tuitions
                        .FirstOrDefaultAsync(t => t.StudentNo == record.StudentNo && t.Term == record.Term);

                    if (existingTuition == null)
                    {
                        var tuition = new Tuition
                        {
                            StudentId = student.Id,
                            StudentNo = record.StudentNo,
                            Term = record.Term,
                            Amount = record.Amount,
                            Balance = record.Amount,
                            IsPaid = false
                        };

                        _context.Tuitions.Add(tuition);
                        await _context.SaveChangesAsync();
                        processedCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing record for student {record.StudentNo}: {ex.Message}");
                }
            }

            var message = $"Batch processing complete. {processedCount} records added successfully.";
            if (errors.Any())
            {
                message += $" {errors.Count} errors occurred.";
            }

            return new TransactionResponse
            {
                Status = "Success",
                Message = message,
                RecordsProcessed = processedCount
            };
        }
        catch (Exception ex)
        {
            return new TransactionResponse
            {
                Status = "Error",
                Message = $"Error processing batch file: {ex.Message}"
            };
        }
    }

    public async Task<UnpaidTuitionResponse> GetUnpaidTuitionsAsync(string term, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Tuitions
            .Include(t => t.Student)
            .Where(t => t.Term == term && !t.IsPaid)
            .OrderBy(t => t.StudentNo);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var tuitions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var students = tuitions.Select(t => new UnpaidStudent
        {
            StudentNo = t.StudentNo,
            Name = t.Student?.Name ?? "Unknown",
            TuitionAmount = t.Amount,
            Balance = t.Balance
        }).ToList();

        return new UnpaidTuitionResponse
        {
            Term = term,
            Students = students,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    private class TuitionCsvRecord
    {
        public string StudentNo { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
