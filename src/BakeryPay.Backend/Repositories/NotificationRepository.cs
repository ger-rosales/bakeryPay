using BakeryPay.Backend.Interfaces.Repositories;
using BakeryPay.Backend.Entities;
using BakeryPay.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryPay.Backend.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(BakeryPayDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Notification>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default) =>
        await Context.Notifications
            .Where(x => x.ProviderId == providerId)
            .OrderByDescending(x => x.SentAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<int> CountUnreadByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default) =>
        await Context.Notifications.CountAsync(x => x.ProviderId == providerId && !x.IsRead, cancellationToken);
}
