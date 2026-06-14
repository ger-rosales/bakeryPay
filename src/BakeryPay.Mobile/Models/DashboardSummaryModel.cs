namespace BakeryPay.Mobile.Models;

public class DashboardSummaryModel
{
    public int TotalProviders { get; set; }
    public int TotalPayments { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public int PendingNotifications { get; set; }
    public List<RecentPaymentModel> RecentPayments { get; set; } = new();
}
