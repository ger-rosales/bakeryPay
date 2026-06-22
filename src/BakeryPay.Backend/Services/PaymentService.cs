using BakeryPay.Backend.Common;
using BakeryPay.Backend.DTOs.Payments;
using BakeryPay.Backend.Interfaces.Repositories;
using BakeryPay.Backend.Interfaces.Services;
using BakeryPay.Backend.Entities;
using BakeryPay.Backend.Enums;

namespace BakeryPay.Backend.Services;

public class PaymentService : IPaymentService
{
    private const string SenderName = "BakeryPay";
    private readonly IPaymentRepository _paymentRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IProviderRepository providerRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _providerRepository = providerRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetAllAsync(cancellationToken);
        return payments.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<PaymentDto>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetByProviderIdAsync(providerId, cancellationToken);
        return payments.Select(Map).ToList();
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        return payment is null ? null : Map(payment);
    }

    public async Task<ServiceResult<PaymentDto>> CreateAsync(Guid registeredByUserId, CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        var validationMessage = ValidateCreate(dto);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return ServiceResult<PaymentDto>.Fail(validationMessage);
        }

        var user = await _userRepository.GetByIdAsync(registeredByUserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return ServiceResult<PaymentDto>.Fail("El usuario que registra el pago no esta disponible.");
        }

        var provider = await _providerRepository.GetByIdAsync(dto.ProviderId, cancellationToken);
        if (provider is null)
        {
            return ServiceResult<PaymentDto>.Fail("Proveedor no encontrado.");
        }

        if (!provider.IsActive)
        {
            return ServiceResult<PaymentDto>.Fail("No se pueden registrar pagos para proveedores inactivos.");
        }

        var normalizedReference = dto.ReferenceNumber.Trim().ToUpperInvariant();
        if (await _paymentRepository.ReferenceExistsAsync(normalizedReference, cancellationToken))
        {
            return ServiceResult<PaymentDto>.Fail("Ya existe un pago con esa referencia.");
        }

        var payment = new Payment
        {
            ProviderId = dto.ProviderId,
            RegisteredByUserId = registeredByUserId,
            Amount = dto.Amount,
            Currency = dto.Currency.Trim().ToUpperInvariant(),
            PaymentDateUtc = dto.PaymentDateUtc,
            ReferenceNumber = normalizedReference,
            Description = dto.Description.Trim(),
            Status = dto.Status
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        await _notificationRepository.AddAsync(
            new Notification
            {
                ProviderId = dto.ProviderId,
                Title = "Nuevo pago registrado",
                Message = $"Se registro el pago {normalizedReference} por {dto.Amount:0.00} {payment.Currency} con estado {GetPaymentStatusDisplay(payment.Status)}.",
                Type = NotificationType.PaymentRegistered,
                RelatedEntityId = payment.Id
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        payment.Provider = provider;
        payment.RegisteredByUser = user;

        var emailResult = await _emailSender.SendAsync(BuildPaymentEmail(provider, payment), cancellationToken);
        var resultMessage = BuildSuccessMessage(
            "Pago registrado correctamente.",
            "Se notifico al proveedor por correo.",
            emailResult);

        return ServiceResult<PaymentDto>.Ok(Map(payment), resultMessage);
    }

    public async Task<ServiceResult<PaymentDto>> UpdateStatusAsync(Guid id, UpdatePaymentStatusDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(dto.Status))
        {
            return ServiceResult<PaymentDto>.Fail("El estado de pago indicado no es valido.");
        }

        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment is null)
        {
            return ServiceResult<PaymentDto>.Fail("Pago no encontrado.");
        }

        if (payment.Status == dto.Status)
        {
            return ServiceResult<PaymentDto>.Ok(Map(payment), "El pago ya se encuentra en ese estado.");
        }

        payment.Status = dto.Status;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        _paymentRepository.Update(payment);

        await _notificationRepository.AddAsync(
            new Notification
            {
                ProviderId = payment.ProviderId,
                Title = "Estado de pago actualizado",
                Message = $"El pago {payment.ReferenceNumber} ahora se encuentra como {GetPaymentStatusDisplay(dto.Status)}.",
                Type = dto.Status == PaymentStatus.Paid ? NotificationType.PaymentApproved : NotificationType.General,
                RelatedEntityId = payment.Id
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var emailResult = await _emailSender.SendAsync(BuildPaymentStatusEmail(payment), cancellationToken);
        var resultMessage = BuildSuccessMessage(
            "Estado del pago actualizado correctamente.",
            "Se notifico al proveedor por correo.",
            emailResult);

        return ServiceResult<PaymentDto>.Ok(Map(payment), resultMessage);
    }

    private static PaymentDto Map(Payment payment) =>
        new()
        {
            Id = payment.Id,
            ProviderId = payment.ProviderId,
            ProviderName = payment.Provider?.BusinessName ?? string.Empty,
            RegisteredByUserId = payment.RegisteredByUserId,
            RegisteredByFullName = payment.RegisteredByUser is null
                ? string.Empty
                : $"{payment.RegisteredByUser.FirstName} {payment.RegisteredByUser.LastName}".Trim(),
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentDateUtc = payment.PaymentDateUtc,
            ReferenceNumber = payment.ReferenceNumber,
            Description = payment.Description,
            Status = payment.Status,
            ReceiptCount = payment.Receipts.Count
        };

    private static string? ValidateCreate(CreatePaymentDto dto)
    {
        if (dto.ProviderId == Guid.Empty)
        {
            return "Selecciona un proveedor valido.";
        }

        if (dto.Amount <= 0)
        {
            return "El monto debe ser mayor que cero.";
        }

        if (string.IsNullOrWhiteSpace(dto.Currency) || dto.Currency.Trim().Length is < 3 or > 5)
        {
            return "La moneda debe tener entre 3 y 5 caracteres.";
        }

        if (string.IsNullOrWhiteSpace(dto.ReferenceNumber))
        {
            return "La referencia del pago es obligatoria.";
        }

        if (dto.ReferenceNumber.Trim().Length > 50)
        {
            return "La referencia no puede superar 50 caracteres.";
        }

        if (dto.Description.Trim().Length > 500)
        {
            return "La descripcion no puede superar 500 caracteres.";
        }

        if (!Enum.IsDefined(dto.Status))
        {
            return "El estado de pago indicado no es valido.";
        }

        if (dto.PaymentDateUtc == default)
        {
            return "La fecha del pago es obligatoria.";
        }

        if (dto.PaymentDateUtc.Date > DateTime.UtcNow.Date.AddDays(1))
        {
            return "La fecha del pago no puede quedar demasiado en el futuro.";
        }

        return null;
    }

    private static EmailMessage BuildPaymentEmail(Provider provider, Payment payment)
    {
        var recipientName = string.IsNullOrWhiteSpace(provider.ContactName) ? provider.BusinessName : provider.ContactName;
        var subject = $"Pago registrado: {payment.ReferenceNumber}";
        var textBody =
$"""
Hola {recipientName},

Se ha registrado un pago para el proveedor {provider.BusinessName}.

Referencia: {payment.ReferenceNumber}
Monto: {payment.Amount:0.00} {payment.Currency}
Fecha: {payment.PaymentDateUtc:dd/MM/yyyy}
Estado: {GetPaymentStatusDisplay(payment.Status)}

Puedes revisar el detalle desde BakeryPay.

Saludos,
{SenderName}
""";

        var htmlBody =
$"""
<p>Hola {recipientName},</p>
<p>Se ha registrado un pago para el proveedor <strong>{provider.BusinessName}</strong>.</p>
<p>
<strong>Referencia:</strong> {payment.ReferenceNumber}<br />
<strong>Monto:</strong> {payment.Amount:0.00} {payment.Currency}<br />
<strong>Fecha:</strong> {payment.PaymentDateUtc:dd/MM/yyyy}<br />
<strong>Estado:</strong> {GetPaymentStatusDisplay(payment.Status)}
</p>
<p>Puedes revisar el detalle desde BakeryPay.</p>
<p>Saludos,<br />{SenderName}</p>
""";

        return new EmailMessage
        {
            ToEmail = provider.Email,
            ToName = recipientName,
            Subject = subject,
            TextBody = textBody,
            HtmlBody = htmlBody
        };
    }

    private static EmailMessage BuildPaymentStatusEmail(Payment payment)
    {
        var provider = payment.Provider!;
        var recipientName = string.IsNullOrWhiteSpace(provider.ContactName) ? provider.BusinessName : provider.ContactName;
        var subject = $"Actualizacion del pago: {payment.ReferenceNumber}";
        var textBody =
$"""
Hola {recipientName},

El estado del pago {payment.ReferenceNumber} fue actualizado.

Monto: {payment.Amount:0.00} {payment.Currency}
Fecha: {payment.PaymentDateUtc:dd/MM/yyyy}
Nuevo estado: {GetPaymentStatusDisplay(payment.Status)}

Puedes revisar el detalle desde BakeryPay.

Saludos,
{SenderName}
""";

        var htmlBody =
$"""
<p>Hola {recipientName},</p>
<p>El estado del pago <strong>{payment.ReferenceNumber}</strong> fue actualizado.</p>
<p>
<strong>Monto:</strong> {payment.Amount:0.00} {payment.Currency}<br />
<strong>Fecha:</strong> {payment.PaymentDateUtc:dd/MM/yyyy}<br />
<strong>Nuevo estado:</strong> {GetPaymentStatusDisplay(payment.Status)}
</p>
<p>Puedes revisar el detalle desde BakeryPay.</p>
<p>Saludos,<br />{SenderName}</p>
""";

        return new EmailMessage
        {
            ToEmail = provider.Email,
            ToName = recipientName,
            Subject = subject,
            TextBody = textBody,
            HtmlBody = htmlBody
        };
    }

    private static string BuildSuccessMessage(string baseMessage, string successSuffix, EmailSendResult emailResult) =>
        emailResult.Success
            ? $"{baseMessage} {successSuffix}"
            : emailResult.IsConfigured
                ? $"{baseMessage} No se pudo enviar el correo: {emailResult.Message}"
                : $"{baseMessage} SMTP pendiente de configurar: {emailResult.Message}";

    private static string GetPaymentStatusDisplay(PaymentStatus status) =>
        status switch
        {
            PaymentStatus.Pending => "Pendiente",
            PaymentStatus.Paid => "Pagado",
            PaymentStatus.Rejected => "Rechazado",
            _ => status.ToString()
        };
}
