using BakeryPay.Backend.Common;
using BakeryPay.Backend.DTOs.Payments;

namespace BakeryPay.Backend.Interfaces.Services;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentDto>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<PaymentDto>> CreateAsync(Guid registeredByUserId, CreatePaymentDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<PaymentDto>> UpdateStatusAsync(Guid id, UpdatePaymentStatusDto dto, CancellationToken cancellationToken = default);
}
