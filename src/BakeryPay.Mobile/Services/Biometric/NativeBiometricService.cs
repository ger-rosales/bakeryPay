namespace BakeryPay.Mobile.Services.Biometric;

public partial class NativeBiometricService : IBiometricService
{
    public partial Task<BiometricAvailability> GetAvailabilityAsync();
    public partial Task<bool> IsAvailableAsync();
    public partial Task<BiometricResult> AuthenticateAsync(string title, string subtitle);
}
