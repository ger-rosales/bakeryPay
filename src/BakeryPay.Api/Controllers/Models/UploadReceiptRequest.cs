using Microsoft.AspNetCore.Mvc;

namespace BakeryPay.Api.Controllers.Models;

public class UploadReceiptRequest
{
    [FromForm(Name = "paymentId")]
    public Guid PaymentId { get; set; }

    [FromForm(Name = "file")]
    public IFormFile File { get; set; } = default!;
}
