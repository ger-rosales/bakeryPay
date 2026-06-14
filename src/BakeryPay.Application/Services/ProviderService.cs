using BakeryPay.Application.Common;
using BakeryPay.Application.DTOs.Providers;
using BakeryPay.Application.Interfaces.Repositories;
using BakeryPay.Application.Interfaces.Security;
using BakeryPay.Application.Interfaces.Services;
using BakeryPay.Domain.Entities;

namespace BakeryPay.Application.Services;

public class ProviderService : IProviderService
{
    private const string ProviderRoleName = "Provider";
    private const string SenderName = "BakeryPay";

    private readonly IProviderRepository _providerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;

    public ProviderService(
        IProviderRepository providerRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender)
    {
        _providerRepository = providerRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
    }

    public async Task<IReadOnlyList<ProviderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var providers = await _providerRepository.GetAllAsync(cancellationToken);
        return providers.Select(Map).ToList();
    }

    public async Task<ProviderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _providerRepository.GetByIdAsync(id, cancellationToken);
        return provider is null ? null : Map(provider);
    }

    public async Task<ServiceResult<CreateProviderResultDto>> CreateAsync(CreateProviderDto dto, CancellationToken cancellationToken = default)
    {
        var validationMessage = Validate(dto);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return ServiceResult<CreateProviderResultDto>.Fail(validationMessage);
        }

        if (await _providerRepository.CodeExistsAsync(dto.Code.Trim(), cancellationToken))
        {
            return ServiceResult<CreateProviderResultDto>.Fail("Ya existe un proveedor con ese codigo.");
        }

        if (await _providerRepository.TaxIdExistsAsync(dto.TaxId.Trim(), cancellationToken))
        {
            return ServiceResult<CreateProviderResultDto>.Fail("Ya existe un proveedor con ese RUC.");
        }

        var normalizedEmail = NormalizeEmail(dto.ContactEmail);
        if (await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return ServiceResult<CreateProviderResultDto>.Fail("Ya existe un usuario con ese correo.");
        }

        if (await _userRepository.IdentificationExistsAsync(dto.ContactIdentificationNumber.Trim(), cancellationToken))
        {
            return ServiceResult<CreateProviderResultDto>.Fail("La identificacion del contacto ya se encuentra registrada.");
        }

        var providerRole = await _roleRepository.GetByNameAsync(ProviderRoleName, cancellationToken);
        if (providerRole is null)
        {
            return ServiceResult<CreateProviderResultDto>.Fail("No existe el rol Provider en la base de datos.");
        }

        var contactName = $"{dto.ContactFirstName.Trim()} {dto.ContactLastName.Trim()}".Trim();
        var provider = new Provider
        {
            Code = dto.Code.Trim(),
            BusinessName = dto.BusinessName.Trim(),
            TaxId = dto.TaxId.Trim(),
            ContactName = contactName,
            ContactIdentificationNumber = dto.ContactIdentificationNumber.Trim(),
            Email = normalizedEmail,
            Phone = dto.ContactPhone.Trim()
        };

        var temporaryPassword = GenerateTemporaryPassword();
        var user = new User
        {
            FirstName = dto.ContactFirstName.Trim(),
            LastName = dto.ContactLastName.Trim(),
            IdentificationNumber = dto.ContactIdentificationNumber.Trim(),
            Email = normalizedEmail,
            Phone = dto.ContactPhone.Trim(),
            PasswordHash = _passwordHasher.Hash(temporaryPassword),
            RoleId = providerRole.Id,
            ProviderId = provider.Id,
            Role = providerRole,
            Provider = provider,
            HasAcceptedPolicies = true,
            MustChangePassword = true
        };

        await _providerRepository.AddAsync(provider, cancellationToken);
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        provider.Users.Add(user);
        var emailResult = await _emailSender.SendAsync(
            BuildAccessEmail(provider, user, temporaryPassword),
            cancellationToken);

        var resultMessage = emailResult.Success
            ? "Proveedor y usuario creados correctamente. Se envio el correo con las credenciales."
            : emailResult.IsConfigured
                ? $"Proveedor y usuario creados correctamente, pero el correo no pudo enviarse: {emailResult.Message}"
                : $"Proveedor y usuario creados correctamente. SMTP pendiente de configurar: {emailResult.Message}";

        return ServiceResult<CreateProviderResultDto>.Ok(
            new CreateProviderResultDto
            {
                Provider = Map(provider),
                UserId = user.Id,
                UserEmail = user.Email,
                TemporaryPassword = temporaryPassword
            },
            resultMessage);
    }

    private static ProviderDto Map(Provider provider)
    {
        var mainUser = provider.Users.FirstOrDefault(x => x.IsActive);
        return new ProviderDto
        {
            Id = provider.Id,
            Code = provider.Code,
            BusinessName = provider.BusinessName,
            TaxId = provider.TaxId,
            ContactName = provider.ContactName,
            ContactIdentificationNumber = provider.ContactIdentificationNumber,
            Email = provider.Email,
            Phone = provider.Phone,
            UserId = mainUser?.Id,
            UserEmail = mainUser?.Email ?? provider.Email
        };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string GenerateTemporaryPassword() =>
        $"BakeryPay#{Guid.NewGuid():N}"[..18];

    private static EmailMessage BuildAccessEmail(Provider provider, User user, string temporaryPassword)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        var subject = "Tus credenciales de acceso a BakeryPay";
        var textBody =
$"""
Hola {fullName},

La empresa panificadora ha creado tu acceso a BakeryPay para el proveedor {provider.BusinessName}.

Correo: {user.Email}
Clave temporal: {temporaryPassword}

Cuando ingreses por primera vez, deberas cambiar tu clave temporal para activar tu acceso.

Saludos,
{SenderName}
""";

        var htmlBody =
$"""
<p>Hola {fullName},</p>
<p>La empresa panificadora ha creado tu acceso a <strong>BakeryPay</strong> para el proveedor <strong>{provider.BusinessName}</strong>.</p>
<p><strong>Correo:</strong> {user.Email}<br />
<strong>Clave temporal:</strong> {temporaryPassword}</p>
<p>Cuando ingreses por primera vez, deberas cambiar tu clave temporal para activar tu acceso.</p>
<p>Saludos,<br />{SenderName}</p>
""";

        return new EmailMessage
        {
            ToEmail = user.Email,
            ToName = fullName,
            Subject = subject,
            TextBody = textBody,
            HtmlBody = htmlBody
        };
    }

    private static string? Validate(CreateProviderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code)
            || string.IsNullOrWhiteSpace(dto.BusinessName)
            || string.IsNullOrWhiteSpace(dto.TaxId)
            || string.IsNullOrWhiteSpace(dto.ContactFirstName)
            || string.IsNullOrWhiteSpace(dto.ContactLastName)
            || string.IsNullOrWhiteSpace(dto.ContactIdentificationNumber)
            || string.IsNullOrWhiteSpace(dto.ContactEmail))
        {
            return "Completa todos los datos del proveedor y su usuario asociado.";
        }

        return null;
    }
}
