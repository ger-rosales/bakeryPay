using BakeryPay.Domain.Enums;

namespace BakeryPay.Application.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAtUtc { get; set; }
    public Guid? RelatedEntityId { get; set; }
}
