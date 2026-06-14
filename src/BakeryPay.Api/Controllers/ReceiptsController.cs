using BakeryPay.Api.Controllers.Models;
using BakeryPay.Api.Extensions;
using BakeryPay.Application.DTOs.Receipts;
using BakeryPay.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryPay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReceiptsController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".png",
        ".jpg",
        ".jpeg"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/png",
        "image/jpeg"
    };

    private readonly IReceiptService _receiptService;
    private readonly IPaymentService _paymentService;
    private readonly IWebHostEnvironment _environment;

    public ReceiptsController(IReceiptService receiptService, IPaymentService paymentService, IWebHostEnvironment environment)
    {
        _receiptService = receiptService;
        _paymentService = paymentService;
        _environment = environment;
    }

    [HttpGet("payment/{paymentId:guid}")]
    public async Task<IActionResult> GetByPayment(Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetByIdAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return NotFound();
        }

        var currentProviderId = User.GetProviderId();
        if (User.IsInRole("Provider") && currentProviderId != payment.ProviderId)
        {
            return Forbid();
        }

        return Ok(await _receiptService.GetByPaymentIdAsync(paymentId, cancellationToken));
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Administrator,Cashier")]
    [RequestSizeLimit(10_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadReceiptRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest("El archivo esta vacio.");
        }

        if (request.File.Length > 10_000_000)
        {
            return BadRequest("El archivo supera el limite de 10 MB.");
        }

        var extension = Path.GetExtension(request.File.FileName);
        if (!AllowedExtensions.Contains(extension) || !AllowedContentTypes.Contains(request.File.ContentType))
        {
            return BadRequest("Solo se permiten archivos PDF, PNG o JPG/JPEG.");
        }

        var uploadsRoot = Path.Combine(_environment.ContentRootPath, "uploads", "receipts");
        Directory.CreateDirectory(uploadsRoot);

        var safeFileName = Path.GetFileName(request.File.FileName);
        var storedFileName = $"{Guid.NewGuid()}_{safeFileName}";
        var fullPath = Path.Combine(uploadsRoot, storedFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        var dto = new CreateReceiptDto
        {
            PaymentId = request.PaymentId,
            FileName = safeFileName,
            StoredFileName = storedFileName,
            ContentType = request.File.ContentType,
            StoragePath = fullPath
        };

        var result = await _receiptService.CreateAsync(dto, cancellationToken);
        if (!result.Success && System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("download/{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var receipt = await _receiptService.GetByIdAsync(id, cancellationToken);
        if (receipt is null || !System.IO.File.Exists(receipt.StoragePath))
        {
            return NotFound();
        }

        var payment = await _paymentService.GetByIdAsync(receipt.PaymentId, cancellationToken);
        if (payment is null)
        {
            return NotFound();
        }

        var currentProviderId = User.GetProviderId();
        if (User.IsInRole("Provider") && currentProviderId != payment.ProviderId)
        {
            return Forbid();
        }

        return PhysicalFile(receipt.StoragePath, receipt.ContentType, receipt.FileName);
    }
}
