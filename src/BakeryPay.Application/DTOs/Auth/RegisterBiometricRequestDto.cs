using BakeryPay.Domain.Enums;

namespace BakeryPay.Application.DTOs.Auth;

public class RegisterBiometricRequestDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public BiometricType BiometricType { get; set; }
}
