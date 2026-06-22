using BakeryPay.Backend.Common;
using BakeryPay.Backend.Enums;

namespace BakeryPay.Backend.Entities;

public class Payment : AuditableEntity
{
    public Guid ProviderId { get; set; }
    public Guid RegisteredByUserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PaymentDateUtc { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;

    public Provider? Provider { get; set; }
    public User? RegisteredByUser { get; set; }
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}
