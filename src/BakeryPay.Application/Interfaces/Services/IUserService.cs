using BakeryPay.Application.Common;
using BakeryPay.Application.DTOs.Users;

namespace BakeryPay.Application.Interfaces.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserDto>> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserDto>> SetStatusAsync(Guid id, SetUserStatusRequestDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<ResetUserPasswordResultDto>> ResetPasswordAsync(Guid id, CancellationToken cancellationToken = default);
}
