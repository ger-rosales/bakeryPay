using BakeryPay.Application.Common;
using BakeryPay.Application.DTOs.Receipts;

namespace BakeryPay.Application.Interfaces.Services;

public interface IReceiptService
{
    Task<ReceiptDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReceiptDto>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<ServiceResult<ReceiptDto>> CreateAsync(CreateReceiptDto dto, CancellationToken cancellationToken = default);
}
