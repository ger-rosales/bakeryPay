using BakeryPay.Application.DTOs.Roles;
using BakeryPay.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryPay.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _roleService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetByIdAsync(id, cancellationToken);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto, CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateAsync(dto, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateAsync(id, dto, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteAsync(id, cancellationToken);
        if (result.Success)
        {
            return Ok(result);
        }

        return string.Equals(result.Message, "Rol no encontrado.", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : BadRequest(result);
    }
}
