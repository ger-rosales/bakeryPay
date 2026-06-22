using BakeryPay.Backend.DTOs.Dashboard;
using BakeryPay.Backend.Entities;

namespace BakeryPay.Backend.Interfaces.Repositories;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecentPaymentDto>> GetRecentPaymentsAsync(int take, CancellationToken cancellationToken = default);
    Task<bool> ReferenceExistsAsync(string referenceNumber, CancellationToken cancellationToken = default);
}
