using BakeryPay.Backend.Enums;

namespace BakeryPay.Backend.DTOs.Auth;

public class BiometricLoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public BiometricType BiometricType { get; set; }
}
