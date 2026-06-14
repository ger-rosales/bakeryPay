using BakeryPay.Domain.Entities;

namespace BakeryPay.Application.Interfaces.Repositories;

public interface IReceiptRepository : IGenericRepository<Receipt>
{
    Task<IReadOnlyList<Receipt>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
}
