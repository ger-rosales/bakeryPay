namespace BakeryPay.Application.DTOs.Receipts;

public class ReceiptDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
}
