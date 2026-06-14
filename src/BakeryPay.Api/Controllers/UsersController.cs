using BakeryPay.Application.DTOs.Users;
using BakeryPay.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryPay.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _userService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateAsync(dto, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateAsync(id, dto, cancellationToken);
        if (result.Success)
        {
            return Ok(result);
        }

        return string.Equals(result.Message, "Usuario no encontrado.", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : BadRequest(result);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] SetUserStatusRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _userService.SetStatusAsync(id, dto, cancellationToken);
        if (result.Success)
        {
            return Ok(result);
        }

        return string.Equals(result.Message, "Usuario no encontrado.", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : BadRequest(result);
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
    {
        var result = await _userService.ResetPasswordAsync(id, cancellationToken);
        if (result.Success)
        {
            return Ok(result);
        }

        return string.Equals(result.Message, "Usuario no encontrado.", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : BadRequest(result);
    }
}
