namespace BakeryPay.Backend.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public bool BiometricEnabled { get; set; }
    public bool HasAcceptedPolicies { get; set; }
    public bool MustChangePassword { get; set; }
}
