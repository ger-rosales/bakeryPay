using BakeryPay.Application.DTOs.Dashboard;
using BakeryPay.Domain.Entities;

namespace BakeryPay.Application.Interfaces.Repositories;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecentPaymentDto>> GetRecentPaymentsAsync(int take, CancellationToken cancellationToken = default);
    Task<bool> ReferenceExistsAsync(string referenceNumber, CancellationToken cancellationToken = default);
}
