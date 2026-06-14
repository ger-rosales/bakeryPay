using System.Security.Claims;
using BakeryPay.Application.DTOs.Auth;
using BakeryPay.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryPay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register-cashier")]
    public async Task<IActionResult> RegisterCashier([FromBody] RegisterCashierRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterCashierAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [AllowAnonymous]
    [HttpPost("biometric-login")]
    public async Task<IActionResult> BiometricLogin([FromBody] BiometricLoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.BiometricLoginAsync(request, cancellationToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(userId.Value, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPost("register-biometric")]
    public async Task<IActionResult> RegisterBiometric([FromBody] RegisterBiometricRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _authService.RegisterBiometricAsync(userId.Value, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimValue, out var userId) ? userId : null;
    }
}
