namespace BakeryPay.Mobile.Models;

public class NotificationModel
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAtUtc { get; set; }

    public string TypeDisplay => Type switch
    {
        "General" => "General",
        "PaymentRegistered" => "Pago registrado",
        "ReceiptUploaded" => "Comprobante cargado",
        "PaymentApproved" => "Pago aprobado",
        _ => Type
    };
}
