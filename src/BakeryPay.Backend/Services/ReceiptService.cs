using BakeryPay.Backend.Common;
using BakeryPay.Backend.DTOs.Receipts;
using BakeryPay.Backend.Interfaces.Repositories;
using BakeryPay.Backend.Interfaces.Services;
using BakeryPay.Backend.Entities;
using BakeryPay.Backend.Enums;

namespace BakeryPay.Backend.Services;

public class ReceiptService : IReceiptService
{
    private const string SenderName = "BakeryPay";
    private readonly IReceiptRepository _receiptRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;

    public ReceiptService(
        IReceiptRepository receiptRepository,
        IPaymentRepository paymentRepository,
        INotificationRepository notificationRepository,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork)
    {
        _receiptRepository = receiptRepository;
        _paymentRepository = paymentRepository;
        _notificationRepository = notificationRepository;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ReceiptDto>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var receipts = await _receiptRepository.GetByPaymentIdAsync(paymentId, cancellationToken);
        return receipts.Select(Map).ToList();
    }

    public async Task<ReceiptDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var receipt = await _receiptRepository.GetByIdAsync(id, cancellationToken);
        return receipt is null ? null : Map(receipt);
    }

    public async Task<ServiceResult<ReceiptDto>> CreateAsync(CreateReceiptDto dto, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(dto.PaymentId, cancellationToken);
        if (payment is null)
        {
            return ServiceResult<ReceiptDto>.Fail("Pago no encontrado.");
        }

        var receipt = new Receipt
        {
            PaymentId = dto.PaymentId,
            FileName = dto.FileName,
            StoredFileName = dto.StoredFileName,
            ContentType = dto.ContentType,
            StoragePath = dto.StoragePath
        };

        await _receiptRepository.AddAsync(receipt, cancellationToken);
        await _notificationRepository.AddAsync(
            new Notification
            {
                ProviderId = payment.ProviderId,
                Title = "Comprobante disponible",
                Message = $"Ya esta disponible el comprobante del pago {payment.ReferenceNumber}.",
                Type = NotificationType.ReceiptUploaded,
                RelatedEntityId = payment.Id
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var emailResult = await _emailSender.SendAsync(BuildReceiptEmail(payment, receipt), cancellationToken);
        var resultMessage = emailResult.Success
            ? "Comprobante cargado correctamente. Se envio por correo al proveedor."
            : emailResult.IsConfigured
                ? $"Comprobante cargado correctamente, pero no se pudo enviar el correo: {emailResult.Message}"
                : $"Comprobante cargado correctamente. SMTP pendiente de configurar: {emailResult.Message}";

        return ServiceResult<ReceiptDto>.Ok(Map(receipt), resultMessage);
    }

    private static ReceiptDto Map(Receipt receipt) =>
        new()
        {
            Id = receipt.Id,
            PaymentId = receipt.PaymentId,
            FileName = receipt.FileName,
            ContentType = receipt.ContentType,
            StoragePath = receipt.StoragePath,
            UploadedAtUtc = receipt.UploadedAtUtc
        };

    private static EmailMessage BuildReceiptEmail(Payment payment, Receipt receipt)
    {
        var providerName = string.IsNullOrWhiteSpace(payment.Provider?.ContactName)
            ? payment.Provider?.BusinessName ?? "Proveedor"
            : payment.Provider!.ContactName;

        var subject = $"Comprobante disponible: {payment.ReferenceNumber}";
        var textBody =
$"""
Hola {providerName},

Se ha cargado el comprobante del pago {payment.ReferenceNumber}.

Monto: {payment.Amount:0.00} {payment.Currency}
Fecha de pago: {payment.PaymentDateUtc:dd/MM/yyyy}
Archivo adjunto: {receipt.FileName}

Tambien puedes revisarlo desde BakeryPay.

Saludos,
{SenderName}
""";

        var htmlBody =
$"""
<p>Hola {providerName},</p>
<p>Se ha cargado el comprobante del pago <strong>{payment.ReferenceNumber}</strong>.</p>
<p>
<strong>Monto:</strong> {payment.Amount:0.00} {payment.Currency}<br />
<strong>Fecha de pago:</strong> {payment.PaymentDateUtc:dd/MM/yyyy}<br />
<strong>Archivo adjunto:</strong> {receipt.FileName}
</p>
<p>Tambien puedes revisarlo desde BakeryPay.</p>
<p>Saludos,<br />{SenderName}</p>
""";

        return new EmailMessage
        {
            ToEmail = payment.Provider?.Email ?? string.Empty,
            ToName = providerName,
            Subject = subject,
            TextBody = textBody,
            HtmlBody = htmlBody,
            Attachments =
            {
                new EmailAttachment
                {
                    FileName = receipt.FileName,
                    ContentType = receipt.ContentType,
                    FilePath = receipt.StoragePath
                }
            }
        };
    }
}
