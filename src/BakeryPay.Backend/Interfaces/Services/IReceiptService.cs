using BakeryPay.Backend.Common;
using BakeryPay.Backend.DTOs.Receipts;

namespace BakeryPay.Backend.Interfaces.Services;

public interface IReceiptService
{
    Task<ReceiptDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReceiptDto>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<ServiceResult<ReceiptDto>> CreateAsync(CreateReceiptDto dto, CancellationToken cancellationToken = default);
}
