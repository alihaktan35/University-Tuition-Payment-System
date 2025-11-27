using UniversityTuitionAPI.DTOs;

namespace UniversityTuitionAPI.Services;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
}
