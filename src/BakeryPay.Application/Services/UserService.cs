using BakeryPay.Application.Common;
using BakeryPay.Application.DTOs.Users;
using BakeryPay.Application.Interfaces.Repositories;
using BakeryPay.Application.Interfaces.Security;
using BakeryPay.Application.Interfaces.Services;
using BakeryPay.Domain.Entities;

namespace BakeryPay.Application.Services;

public class UserService : IUserService
{
    private const string AdministratorRoleName = "Administrator";
    private const string ProviderRoleName = "Provider";
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(Map).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var validationMessage = ValidateForCreate(dto);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return ServiceResult<UserDto>.Fail(validationMessage);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return ServiceResult<UserDto>.Fail("Ya existe un usuario con ese correo.");
        }

        if (await _userRepository.IdentificationExistsAsync(dto.IdentificationNumber.Trim(), cancellationToken))
        {
            return ServiceResult<UserDto>.Fail("La identificacion ya existe en otro usuario.");
        }

        var role = await _roleRepository.GetByIdAsync(dto.RoleId, cancellationToken);
        if (role is null)
        {
            return ServiceResult<UserDto>.Fail("El rol indicado no existe.");
        }

        if (string.Equals(role.Name, ProviderRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<UserDto>.Fail("Los usuarios proveedor se crean desde el modulo de proveedores.");
        }

        if (string.Equals(role.Name, AdministratorRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<UserDto>.Fail("El usuario administrador principal se configura internamente.");
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            IdentificationNumber = dto.IdentificationNumber.Trim(),
            Email = normalizedEmail,
            Phone = dto.Phone.Trim(),
            RoleId = dto.RoleId,
            ProviderId = dto.ProviderId,
            Role = role,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            MustChangePassword = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserDto>.Ok(Map(user), "Usuario creado correctamente.");
    }

    public async Task<ServiceResult<UserDto>> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<UserDto>.Fail("Usuario no encontrado.");
        }

        var validationMessage = ValidateForUpdate(dto);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return ServiceResult<UserDto>.Fail(validationMessage);
        }

        var role = await _roleRepository.GetByIdAsync(dto.RoleId, cancellationToken);
        if (role is null)
        {
            return ServiceResult<UserDto>.Fail("El rol indicado no existe.");
        }

        if (string.Equals(role.Name, ProviderRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<UserDto>.Fail("Los usuarios proveedor se crean desde el modulo de proveedores.");
        }

        var currentRoleName = user.Role?.Name ?? string.Empty;
        var targetIsAdministrator = string.Equals(role.Name, AdministratorRoleName, StringComparison.OrdinalIgnoreCase);
        var currentIsAdministrator = string.Equals(currentRoleName, AdministratorRoleName, StringComparison.OrdinalIgnoreCase);

        if (targetIsAdministrator && !currentIsAdministrator)
        {
            return ServiceResult<UserDto>.Fail("El rol Administrator es exclusivo del usuario configurado internamente.");
        }

        if (!targetIsAdministrator && currentIsAdministrator)
        {
            return ServiceResult<UserDto>.Fail("No se puede cambiar el rol del administrador principal desde este modulo.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)
            && await _userRepository.EmailExistsAsync(normalizedEmail, user.Id, cancellationToken))
        {
            return ServiceResult<UserDto>.Fail("Ya existe un usuario con ese correo.");
        }

        if (!string.Equals(user.IdentificationNumber, dto.IdentificationNumber.Trim(), StringComparison.OrdinalIgnoreCase)
            && await _userRepository.IdentificationExistsAsync(dto.IdentificationNumber.Trim(), cancellationToken))
        {
            return ServiceResult<UserDto>.Fail("La identificacion ya existe en otro usuario.");
        }

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.IdentificationNumber = dto.IdentificationNumber.Trim();
        user.Email = normalizedEmail;
        user.Phone = dto.Phone.Trim();
        user.RoleId = dto.RoleId;
        user.Role = role;
        user.UpdatedAtUtc = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserDto>.Ok(Map(user), "Usuario actualizado correctamente.");
    }

    public async Task<ServiceResult<UserDto>> SetStatusAsync(Guid id, SetUserStatusRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<UserDto>.Fail("Usuario no encontrado.");
        }

        if (string.Equals(user.Role?.Name, AdministratorRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<UserDto>.Fail("El administrador principal no puede activarse ni desactivarse desde este modulo.");
        }

        user.IsActive = dto.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserDto>.Ok(
            Map(user),
            dto.IsActive ? "Usuario activado correctamente." : "Usuario desactivado correctamente.");
    }

    public async Task<ServiceResult<ResetUserPasswordResultDto>> ResetPasswordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<ResetUserPasswordResultDto>.Fail("Usuario no encontrado.");
        }

        if (string.Equals(user.Role?.Name, AdministratorRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<ResetUserPasswordResultDto>.Fail("La clave del administrador principal no se resetea desde este modulo.");
        }

        var temporaryPassword = GenerateTemporaryPassword();
        user.PasswordHash = _passwordHasher.Hash(temporaryPassword);
        user.MustChangePassword = true;
        user.PasswordChangedAtUtc = null;
        user.UpdatedAtUtc = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<ResetUserPasswordResultDto>.Ok(
            new ResetUserPasswordResultDto
            {
                UserId = user.Id,
                Email = user.Email,
                TemporaryPassword = temporaryPassword
            },
            "Clave temporal generada correctamente.");
    }

    private static UserDto Map(User user) =>
        new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IdentificationNumber = user.IdentificationNumber,
            Email = user.Email,
            Phone = user.Phone,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty,
            BiometricEnabled = user.BiometricCredentials.Any(x => x.IsActive),
            MustChangePassword = user.MustChangePassword,
            IsActive = user.IsActive,
            ProviderId = user.ProviderId
        };

    private static string? ValidateForCreate(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName)
            || string.IsNullOrWhiteSpace(dto.LastName)
            || string.IsNullOrWhiteSpace(dto.IdentificationNumber)
            || string.IsNullOrWhiteSpace(dto.Email)
            || string.IsNullOrWhiteSpace(dto.Phone)
            || string.IsNullOrWhiteSpace(dto.Password))
        {
            return "Completa nombres, apellidos, identificacion, correo, telefono y clave.";
        }

        if (dto.Password.Trim().Length < 8)
        {
            return "La clave inicial debe tener al menos 8 caracteres.";
        }

        return null;
    }

    private static string? ValidateForUpdate(UpdateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName)
            || string.IsNullOrWhiteSpace(dto.LastName)
            || string.IsNullOrWhiteSpace(dto.IdentificationNumber)
            || string.IsNullOrWhiteSpace(dto.Email)
            || string.IsNullOrWhiteSpace(dto.Phone))
        {
            return "Completa nombres, apellidos, identificacion, correo y telefono.";
        }

        return null;
    }

    private static string GenerateTemporaryPassword() => $"BakeryPay#{Guid.NewGuid():N}"[..18];
}
