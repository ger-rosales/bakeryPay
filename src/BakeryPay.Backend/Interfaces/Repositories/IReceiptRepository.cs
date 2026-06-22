using BakeryPay.Backend.Entities;

namespace BakeryPay.Backend.Interfaces.Repositories;

public interface IReceiptRepository : IGenericRepository<Receipt>
{
    Task<IReadOnlyList<Receipt>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
}
