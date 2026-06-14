using Microsoft.Maui.Graphics;

namespace BakeryPay.Mobile.Models;

public class PaymentModel
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public Guid RegisteredByUserId { get; set; }
    public string RegisteredByFullName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime PaymentDateUtc { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ReceiptCount { get; set; }

    public string ProviderInitials => BuildInitials(ProviderName);

    public string StatusDisplay => Status switch
    {
        "Pending" => "Pendiente",
        "Paid" => "Pagado",
        "Rejected" => "Rechazado",
        _ => Status
    };

    public Color StatusColor => Status switch
    {
        "Pending" => Color.FromArgb("#D29A19"),
        "Paid" => Color.FromArgb("#1FA971"),
        "Rejected" => Color.FromArgb("#D64545"),
        _ => Color.FromArgb("#6E7787")
    };

    private static string BuildInitials(string value)
    {
        var parts = value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(x => char.ToUpperInvariant(x[0]));

        var result = string.Concat(parts);
        return string.IsNullOrWhiteSpace(result) ? "PG" : result;
    }
}
