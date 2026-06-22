using BakeryPay.Backend.Enums;

namespace BakeryPay.Backend.DTOs.Payments;

public class CreatePaymentDto
{
    public Guid ProviderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PaymentDateUtc { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;
}
