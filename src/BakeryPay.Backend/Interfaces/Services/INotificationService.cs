using BakeryPay.Backend.Common;
using BakeryPay.Backend.DTOs.Notifications;

namespace BakeryPay.Backend.Interfaces.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<NotificationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<NotificationDto>> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
}
