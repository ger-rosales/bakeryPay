using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services.Biometric;

public class BiometricAvailability
{
    public bool IsAvailable { get; set; }
    public BiometricTypeOption BiometricType { get; set; }
    public string DisplayName { get; set; } = "Biometria";
}
