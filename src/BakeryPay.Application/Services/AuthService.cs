using BakeryPay.Application.Common;
using BakeryPay.Application.DTOs.Auth;
using BakeryPay.Application.Interfaces.Repositories;
using BakeryPay.Application.Interfaces.Security;
using BakeryPay.Application.Interfaces.Services;
using BakeryPay.Domain.Entities;
using BakeryPay.Domain.Enums;

namespace BakeryPay.Application.Services;

public class AuthService : IAuthService
{
    private const string CashierRoleName = "Cashier";
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserBiometricRepository _userBiometricRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserBiometricRepository userBiometricRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userBiometricRepository = userBiometricRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterCashierAsync(RegisterCashierRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName)
            || string.IsNullOrWhiteSpace(request.LastName)
            || string.IsNullOrWhiteSpace(request.IdentificationNumber)
            || string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Phone)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            return ServiceResult<AuthResponseDto>.Fail("Completa nombres, apellidos, identificacion, telefono, correo y clave para registrarte.");
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return ServiceResult<AuthResponseDto>.Fail("La confirmacion de la clave no coincide.");
        }

        if (request.Password.Length < 8)
        {
            return ServiceResult<AuthResponseDto>.Fail("La clave debe tener al menos 8 caracteres.");
        }

        if (!request.AcceptPolicies)
        {
            return ServiceResult<AuthResponseDto>.Fail("Debes aceptar las politicas para completar el registro.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        if (await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return ServiceResult<AuthResponseDto>.Fail("Ya existe un usuario con ese correo.");
        }

        var normalizedIdentification = request.IdentificationNumber.Trim();
        if (await _userRepository.IdentificationExistsAsync(normalizedIdentification, cancellationToken))
        {
            return ServiceResult<AuthResponseDto>.Fail("La identificacion ya se encuentra registrada en otro usuario.");
        }

        var cashierRole = await _roleRepository.GetByNameAsync(CashierRoleName, cancellationToken);
        if (cashierRole is null)
        {
            return ServiceResult<AuthResponseDto>.Fail("No existe el rol Cashier configurado en la base de datos.");
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IdentificationNumber = normalizedIdentification,
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Phone = request.Phone.Trim(),
            RoleId = cashierRole.Id,
            Role = cashierRole,
            HasAcceptedPolicies = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthResponseDto>.Ok(
            _jwtTokenGenerator.GenerateToken(user),
            "Cuenta de cajera creada correctamente.");
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return ServiceResult<AuthResponseDto>.Fail("Credenciales invalidas.");
        }

        return ServiceResult<AuthResponseDto>.Ok(
            _jwtTokenGenerator.GenerateToken(user),
            "Inicio de sesion exitoso.");
    }

    public async Task<ServiceResult<AuthResponseDto>> BiometricLoginAsync(BiometricLoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null)
        {
            return ServiceResult<AuthResponseDto>.Fail("Usuario no encontrado.");
        }

        var credential = await _userBiometricRepository.GetActiveByUserAndDeviceAsync(user.Id, request.DeviceId, cancellationToken);
        if (credential is null)
        {
            return ServiceResult<AuthResponseDto>.Fail("No existe un enrolamiento biometrico activo para este dispositivo.");
        }

        if (credential.BiometricType != BiometricType.DeviceBiometric
            && request.BiometricType != BiometricType.DeviceBiometric
            && credential.BiometricType != request.BiometricType)
        {
            return ServiceResult<AuthResponseDto>.Fail("El tipo de biometria no coincide con el enrolado.");
        }

        credential.LastUsedAtUtc = DateTime.UtcNow;
        credential.UpdatedAtUtc = DateTime.UtcNow;
        _userBiometricRepository.Update(credential);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        user.BiometricCredentials = (await _userBiometricRepository.GetByUserIdAsync(user.Id, cancellationToken)).ToList();
        return ServiceResult<AuthResponseDto>.Ok(
            _jwtTokenGenerator.GenerateToken(user),
            "Autenticacion biometrica exitosa.");
    }

    public async Task<ServiceResult<AuthResponseDto>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ServiceResult<AuthResponseDto>.Fail("Usuario no encontrado.");
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword)
            || string.IsNullOrWhiteSpace(request.NewPassword)
            || string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
        {
            return ServiceResult<AuthResponseDto>.Fail("Completa la clave actual, la nueva clave y su confirmacion.");
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return ServiceResult<AuthResponseDto>.Fail("La clave actual no es correcta.");
        }

        if (!string.Equals(request.NewPassword, request.ConfirmNewPassword, StringComparison.Ordinal))
        {
            return ServiceResult<AuthResponseDto>.Fail("La confirmacion de la nueva clave no coincide.");
        }

        if (request.NewPassword.Length < 8)
        {
            return ServiceResult<AuthResponseDto>.Fail("La nueva clave debe tener al menos 8 caracteres.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.MustChangePassword = false;
        user.PasswordChangedAtUtc = DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthResponseDto>.Ok(
            _jwtTokenGenerator.GenerateToken(user),
            "Clave actualizada correctamente.");
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterBiometricAsync(Guid userId, RegisterBiometricRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ServiceResult<AuthResponseDto>.Fail("Usuario no encontrado.");
        }

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            return ServiceResult<AuthResponseDto>.Fail("No se recibio un identificador de dispositivo valido.");
        }

        var credential = await _userBiometricRepository.GetActiveByUserAndDeviceAsync(user.Id, request.DeviceId.Trim(), cancellationToken);
        if (credential is null)
        {
            credential = new UserBiometricCredential
            {
                UserId = user.Id,
                DeviceId = request.DeviceId.Trim(),
                DeviceName = string.IsNullOrWhiteSpace(request.DeviceName) ? "Dispositivo movil" : request.DeviceName.Trim(),
                Platform = string.IsNullOrWhiteSpace(request.Platform) ? "Unknown" : request.Platform.Trim(),
                BiometricType = request.BiometricType,
                EnrolledAtUtc = DateTime.UtcNow
            };

            await _userBiometricRepository.AddAsync(credential, cancellationToken);
        }
        else
        {
            credential.DeviceName = string.IsNullOrWhiteSpace(request.DeviceName) ? credential.DeviceName : request.DeviceName.Trim();
            credential.Platform = string.IsNullOrWhiteSpace(request.Platform) ? credential.Platform : request.Platform.Trim();
            credential.BiometricType = request.BiometricType;
            credential.UpdatedAtUtc = DateTime.UtcNow;
            _userBiometricRepository.Update(credential);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        user.BiometricCredentials = (await _userBiometricRepository.GetByUserIdAsync(user.Id, cancellationToken)).ToList();
        return ServiceResult<AuthResponseDto>.Ok(
            _jwtTokenGenerator.GenerateToken(user),
            "Biometria registrada correctamente.");
    }
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
