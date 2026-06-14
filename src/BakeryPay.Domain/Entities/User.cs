using BakeryPay.Domain.Common;

namespace BakeryPay.Domain.Entities;

public class User : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public bool HasAcceptedPolicies { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? PasswordChangedAtUtc { get; set; }
    public Guid? ProviderId { get; set; }

    public Role? Role { get; set; }
    public Provider? Provider { get; set; }
    public ICollection<Payment> RegisteredPayments { get; set; } = new List<Payment>();
    public ICollection<UserBiometricCredential> BiometricCredentials { get; set; } = new List<UserBiometricCredential>();
}
