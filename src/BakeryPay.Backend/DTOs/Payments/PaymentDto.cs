using BakeryPay.Backend.Enums;

namespace BakeryPay.Backend.DTOs.Payments;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public Guid RegisteredByUserId { get; set; }
    public string RegisteredByFullName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PaymentDateUtc { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public int ReceiptCount { get; set; }
}
