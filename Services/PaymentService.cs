using Microsoft.EntityFrameworkCore;
using UniversityTuitionAPI.Data;
using UniversityTuitionAPI.DTOs;
using UniversityTuitionAPI.Models;

namespace UniversityTuitionAPI.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var tuition = await _context.Tuitions
                .FirstOrDefaultAsync(t => t.StudentNo == request.StudentNo && t.Term == request.Term);

            if (tuition == null)
            {
                return new PaymentResponse
                {
                    Status = "Error",
                    Message = "Tuition record not found for the specified student and term"
                };
            }

            if (tuition.IsPaid)
            {
                return new PaymentResponse
                {
                    Status = "Error",
                    Message = "Tuition is already fully paid",
                    RemainingBalance = 0
                };
            }

            if (request.Amount <= 0)
            {
                return new PaymentResponse
                {
                    Status = "Error",
                    Message = "Payment amount must be greater than zero"
                };
            }

            if (request.Amount > tuition.Balance)
            {
                return new PaymentResponse
                {
                    Status = "Error",
                    Message = $"Payment amount exceeds remaining balance of {tuition.Balance:C}"
                };
            }

            // Process payment
            tuition.Balance -= request.Amount;

            var paymentStatus = tuition.Balance == 0 ? "Successful" : "Partial";

            if (tuition.Balance == 0)
            {
                tuition.IsPaid = true;
                tuition.PaidAt = DateTime.UtcNow;
            }

            // Record payment
            var payment = new Payment
            {
                TuitionId = tuition.Id,
                StudentId = tuition.StudentId,
                StudentNo = request.StudentNo,
                Term = request.Term,
                Amount = request.Amount,
                Status = paymentStatus,
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            _context.Tuitions.Update(tuition);
            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                Status = paymentStatus,
                AmountPaid = request.Amount,
                RemainingBalance = tuition.Balance,
                Message = paymentStatus == "Successful"
                    ? "Payment processed successfully. Tuition fully paid."
                    : $"Partial payment processed. Remaining balance: {tuition.Balance:C}"
            };
        }
        catch (Exception ex)
        {
            return new PaymentResponse
            {
                Status = "Error",
                Message = $"An error occurred while processing payment: {ex.Message}"
            };
        }
    }
}
