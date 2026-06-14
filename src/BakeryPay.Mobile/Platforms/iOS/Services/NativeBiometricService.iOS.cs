#if IOS
using LocalAuthentication;
using Microsoft.Maui.ApplicationModel;
using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services.Biometric;

public partial class NativeBiometricService
{
    public partial Task<BiometricAvailability> GetAvailabilityAsync()
    {
        var context = new LAContext();
        var available = context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out _);

        return Task.FromResult(new BiometricAvailability
        {
            IsAvailable = available,
            BiometricType = available
                ? MapBiometricType(context.BiometryType)
                : BiometricTypeOption.None,
            DisplayName = context.BiometryType == LABiometryType.FaceId
                ? "Reconocimiento facial"
                : "Huella digital"
        });
    }

    public partial Task<bool> IsAvailableAsync()
    {
        return GetAvailabilityAsync().ContinueWith(task => task.Result.IsAvailable);
    }

    public partial Task<BiometricResult> AuthenticateAsync(string title, string subtitle)
    {
        var tcs = new TaskCompletionSource<BiometricResult>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var context = new LAContext
            {
                LocalizedFallbackTitle = "Usar clave"
            };

            var reason = $"{title}: {subtitle}";
            context.EvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, reason, (success, error) =>
            {
                tcs.TrySetResult(new BiometricResult
                {
                    Success = success,
                    Message = success
                        ? "Autenticación biométrica exitosa."
                        : error?.LocalizedDescription ?? "No fue posible validar la biometría."
                });
            });
        });

        return tcs.Task;
    }

    private static BiometricTypeOption MapBiometricType(LABiometryType biometryType) =>
        biometryType switch
        {
            LABiometryType.FaceId => BiometricTypeOption.FaceRecognition,
            LABiometryType.TouchId => BiometricTypeOption.Fingerprint,
            _ => BiometricTypeOption.DeviceBiometric
        };
}
#endif
