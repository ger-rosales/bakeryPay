namespace BakeryPay.Application.DTOs.Receipts;

public class CreateReceiptDto
{
    public Guid PaymentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
}
