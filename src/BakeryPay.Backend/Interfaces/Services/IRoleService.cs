using BakeryPay.Backend.Common;
using BakeryPay.Backend.DTOs.Roles;

namespace BakeryPay.Backend.Interfaces.Services;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<RoleDto>> UpdateAsync(Guid id, UpdateRoleDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
