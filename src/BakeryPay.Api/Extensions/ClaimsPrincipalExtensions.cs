using System.Security.Claims;

namespace BakeryPay.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(ClaimTypes.Name)
            ?? user.FindFirstValue(ClaimTypes.Sid)
            ?? user.FindFirstValue("sub");

        return Guid.TryParse(value, out var userId)
            ? userId
            : Guid.Empty;
    }

    public static Guid? GetProviderId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("provider_id");
        return Guid.TryParse(value, out var providerId) ? providerId : null;
    }
}
