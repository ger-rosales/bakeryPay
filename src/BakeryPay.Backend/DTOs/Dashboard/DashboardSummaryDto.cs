namespace BakeryPay.Backend.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalProviders { get; set; }
    public int TotalPayments { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public int PendingNotifications { get; set; }
    public List<RecentPaymentDto> RecentPayments { get; set; } = new();
}
