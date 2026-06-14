namespace BakeryPay.Application.DTOs.Dashboard;

public class RecentPaymentDto
{
    public Guid PaymentId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDateUtc { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}
