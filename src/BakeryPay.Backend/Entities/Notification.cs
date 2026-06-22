using BakeryPay.Backend.Common;
using BakeryPay.Backend.Enums;

namespace BakeryPay.Backend.Entities;

public class Notification : AuditableEntity
{
    public Guid ProviderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.General;
    public bool IsRead { get; set; }
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? RelatedEntityId { get; set; }

    public Provider? Provider { get; set; }
}
