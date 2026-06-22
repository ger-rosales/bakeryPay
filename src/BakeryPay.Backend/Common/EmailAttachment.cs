namespace BakeryPay.Backend.Common;

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public string FilePath { get; set; } = string.Empty;
}
