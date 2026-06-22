using BakeryPay.Backend.Common;

namespace BakeryPay.Backend.Entities;

public class Receipt : AuditableEntity
{
    public Guid PaymentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public Payment? Payment { get; set; }
}
