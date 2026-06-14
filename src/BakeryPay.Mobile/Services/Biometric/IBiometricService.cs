namespace BakeryPay.Mobile.Services.Biometric;

public interface IBiometricService
{
    Task<BiometricAvailability> GetAvailabilityAsync();
    Task<bool> IsAvailableAsync();
    Task<BiometricResult> AuthenticateAsync(string title, string subtitle);
}
