using BakeryPay.Domain.Entities;

namespace BakeryPay.Application.Interfaces.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<int> CountUnreadByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
}
