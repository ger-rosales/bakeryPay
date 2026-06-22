using BakeryPay.Backend.Extensions;
using BakeryPay.Backend.DTOs.Notifications;
using BakeryPay.Backend.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryPay.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("provider/{providerId:guid}")]
    public async Task<IActionResult> GetByProvider(Guid providerId, CancellationToken cancellationToken)
    {
        var currentProviderId = User.GetProviderId();
        if (User.IsInRole("Provider") && currentProviderId != providerId)
        {
            return Forbid();
        }

        return Ok(await _notificationService.GetByProviderIdAsync(providerId, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Cashier")]
    public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto, CancellationToken cancellationToken)
    {
        var result = await _notificationService.CreateAsync(dto, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.GetByIdAsync(id, cancellationToken);
        if (notification is null)
        {
            return NotFound();
        }

        var currentProviderId = User.GetProviderId();
        if (User.IsInRole("Provider") && currentProviderId != notification.ProviderId)
        {
            return Forbid();
        }

        var result = await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
