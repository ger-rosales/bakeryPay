using BakeryPay.Backend.Extensions;
using BakeryPay.Backend.DTOs.Providers;
using BakeryPay.Backend.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryPay.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator,Cashier")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _providerService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var provider = await _providerService.GetByIdAsync(id, cancellationToken);
        if (provider is null)
        {
            return NotFound();
        }

        var currentProviderId = User.GetProviderId();
        if (User.IsInRole("Provider") && currentProviderId != provider.Id)
        {
            return Forbid();
        }

        return Ok(provider);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Cashier")]
    public async Task<IActionResult> Create([FromBody] CreateProviderDto dto, CancellationToken cancellationToken)
    {
        var result = await _providerService.CreateAsync(dto, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,Cashier")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProviderDto dto, CancellationToken cancellationToken)
    {
        var result = await _providerService.UpdateAsync(id, dto, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
