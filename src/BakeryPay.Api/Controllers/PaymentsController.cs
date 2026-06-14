using BakeryPay.Api.Extensions;
using BakeryPay.Application.DTOs.Payments;
using BakeryPay.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryPay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator,Cashier")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _paymentService.GetAllAsync(cancellationToken));

    [HttpGet("provider/{providerId:guid}")]
    public async Task<IActionResult> GetByProvider(Guid providerId, CancellationToken cancellationToken)
    {
        var currentProviderId = User.GetProviderId();
        if (User.IsInRole("Provider") && currentProviderId != providerId)
        {
            return Forbid();
        }

        return Ok(await _paymentService.GetByProviderIdAsync(providerId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetByIdAsync(id, cancellationToken);
        if (payment is null)
        {
            return NotFound();
        }

        var currentProviderId = User.GetProviderId();
        if (User.IsInRole("Provider") && currentProviderId != payment.ProviderId)
        {
            return Forbid();
        }

        return Ok(payment);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Cashier")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        var result = await _paymentService.CreateAsync(User.GetUserId(), dto, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Administrator,Cashier")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePaymentStatusDto dto, CancellationToken cancellationToken)
    {
        var result = await _paymentService.UpdateStatusAsync(id, dto, cancellationToken);
        if (result.Success)
        {
            return Ok(result);
        }

        return string.Equals(result.Message, "Pago no encontrado.", StringComparison.OrdinalIgnoreCase)
            ? NotFound(result)
            : BadRequest(result);
    }
}
