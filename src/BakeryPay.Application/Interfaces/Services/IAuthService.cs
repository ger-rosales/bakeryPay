using BakeryPay.Application.Common;
using BakeryPay.Application.DTOs.Auth;

namespace BakeryPay.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> RegisterCashierAsync(RegisterCashierRequestDto request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthResponseDto>> BiometricLoginAsync(BiometricLoginRequestDto request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthResponseDto>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthResponseDto>> RegisterBiometricAsync(Guid userId, RegisterBiometricRequestDto request, CancellationToken cancellationToken = default);
}
