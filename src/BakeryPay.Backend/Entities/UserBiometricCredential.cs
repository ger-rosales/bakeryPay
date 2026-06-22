using BakeryPay.Backend.Common;
using BakeryPay.Backend.Enums;

namespace BakeryPay.Backend.Entities;

public class UserBiometricCredential : AuditableEntity
{
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public BiometricType BiometricType { get; set; }
    public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAtUtc { get; set; }

    public User? User { get; set; }
}
