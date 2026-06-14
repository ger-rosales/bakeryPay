using BakeryPay.Application.Common;
using BakeryPay.Application.DTOs.Notifications;

namespace BakeryPay.Application.Interfaces.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<NotificationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<NotificationDto>> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
}
