using BakeryPay.Backend.Common;
using BakeryPay.Backend.DTOs.Roles;
using BakeryPay.Backend.Interfaces.Repositories;
using BakeryPay.Backend.Interfaces.Services;
using BakeryPay.Backend.Entities;

namespace BakeryPay.Backend.Services;

public class RoleService : IRoleService
{
    private const string AdministratorRoleName = "Administrator";
    private const string CashierRoleName = "Cashier";
    private const string ProviderRoleName = "Provider";
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        return roles.Select(Map).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        return role is null ? null : Map(role);
    }

    public async Task<ServiceResult<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var validationMessage = Validate(dto.Name, dto.Description);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return ServiceResult<RoleDto>.Fail(validationMessage);
        }

        if (await _roleRepository.GetByNameAsync(dto.Name.Trim(), cancellationToken) is not null)
        {
            return ServiceResult<RoleDto>.Fail("Ya existe un rol con ese nombre.");
        }

        var role = new Role
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            IsSystemRole = false
        };

        await _roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<RoleDto>.Ok(Map(role), "Rol creado correctamente.");
    }

    public async Task<ServiceResult<RoleDto>> UpdateAsync(Guid id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return ServiceResult<RoleDto>.Fail("Rol no encontrado.");
        }

        var validationMessage = Validate(dto.Name, dto.Description);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return ServiceResult<RoleDto>.Fail(validationMessage);
        }

        var existingRole = await _roleRepository.GetByNameAsync(dto.Name.Trim(), cancellationToken);
        if (existingRole is not null && existingRole.Id != id)
        {
            return ServiceResult<RoleDto>.Fail("Ya existe un rol con ese nombre.");
        }

        var normalizedName = dto.Name.Trim();
        if (role.IsSystemRole && !string.Equals(role.Name, normalizedName, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<RoleDto>.Fail("Los roles base del sistema no permiten cambiar su nombre.");
        }

        role.Name = normalizedName;
        role.Description = dto.Description.Trim();

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<RoleDto>.Ok(Map(role), "Rol actualizado correctamente.");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return ServiceResult<bool>.Fail("Rol no encontrado.");
        }

        if (role.IsSystemRole)
        {
            return ServiceResult<bool>.Fail("Los roles base del sistema no pueden eliminarse.");
        }

        if (role.Users.Any())
        {
            return ServiceResult<bool>.Fail("No puedes eliminar un rol que ya esta asignado a usuarios.");
        }

        _roleRepository.Remove(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Ok(true, "Rol eliminado correctamente.");
    }

    private static RoleDto Map(Role role) =>
        new()
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            AssignedUsersCount = role.Users.Count
        };

    private static string? Validate(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
        {
            return "Completa el nombre y la descripcion del rol.";
        }

        if (name.Length > 50)
        {
            return "El nombre del rol no puede superar 50 caracteres.";
        }

        if (description.Length > 200)
        {
            return "La descripcion del rol no puede superar 200 caracteres.";
        }

        if (string.Equals(name.Trim(), AdministratorRoleName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(name.Trim(), CashierRoleName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(name.Trim(), ProviderRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return null;
    }
}
