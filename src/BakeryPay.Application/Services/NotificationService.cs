using BakeryPay.Application.Common;
using BakeryPay.Application.DTOs.Notifications;
using BakeryPay.Application.Interfaces.Repositories;
using BakeryPay.Application.Interfaces.Services;
using BakeryPay.Domain.Entities;

namespace BakeryPay.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetByProviderIdAsync(providerId, cancellationToken);
        return notifications.Select(Map).ToList();
    }

    public async Task<NotificationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
        return notification is null ? null : Map(notification);
    }

    public async Task<ServiceResult<NotificationDto>> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            ProviderId = dto.ProviderId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            RelatedEntityId = dto.RelatedEntityId
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<NotificationDto>.Ok(Map(notification), "Notificación creada correctamente.");
    }

    public async Task<ServiceResult<bool>> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
        if (notification is null)
        {
            return ServiceResult<bool>.Fail("Notificación no encontrada.");
        }

        notification.IsRead = true;
        notification.UpdatedAtUtc = DateTime.UtcNow;

        _notificationRepository.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Ok(true, "Notificación marcada como leída.");
    }

    private static NotificationDto Map(Notification notification) =>
        new()
        {
            Id = notification.Id,
            ProviderId = notification.ProviderId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            SentAtUtc = notification.SentAtUtc,
            RelatedEntityId = notification.RelatedEntityId
        };
}
