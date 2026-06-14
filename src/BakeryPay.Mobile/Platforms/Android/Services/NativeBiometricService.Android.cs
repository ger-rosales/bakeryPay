#if ANDROID
using AndroidX.Biometric;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using BakeryPay.Mobile.Models;
using Microsoft.Maui.ApplicationModel;

namespace BakeryPay.Mobile.Services.Biometric;

public partial class NativeBiometricService
{
    public partial Task<BiometricAvailability> GetAvailabilityAsync()
    {
        var activity = Platform.CurrentActivity as FragmentActivity;
        if (activity is null)
        {
            return Task.FromResult(new BiometricAvailability());
        }

        var result = BiometricManager.From(activity).CanAuthenticate(BiometricManager.Authenticators.BiometricStrong);
        return Task.FromResult(new BiometricAvailability
        {
            IsAvailable = result == BiometricManager.BiometricSuccess,
            BiometricType = result == BiometricManager.BiometricSuccess
                ? BiometricTypeOption.DeviceBiometric
                : BiometricTypeOption.None,
            DisplayName = "Biometria del dispositivo"
        });
    }

    public partial Task<bool> IsAvailableAsync()
    {
        return GetAvailabilityAsync().ContinueWith(task => task.Result.IsAvailable);
    }

    public partial Task<BiometricResult> AuthenticateAsync(string title, string subtitle)
    {
        var activity = Platform.CurrentActivity as FragmentActivity;
        if (activity is null)
        {
            return Task.FromResult(new BiometricResult
            {
                Success = false,
                Message = "No hay una actividad Android disponible para biometria."
            });
        }

        var tcs = new TaskCompletionSource<BiometricResult>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var executor = ContextCompat.GetMainExecutor(activity);
            if (executor is null)
            {
                tcs.TrySetResult(new BiometricResult
                {
                    Success = false,
                    Message = "No fue posible inicializar la biometria en Android."
                });
                return;
            }

            var callback = new PromptCallback(tcs);
            var prompt = new BiometricPrompt(activity, executor, callback);
            var promptInfo = new BiometricPrompt.PromptInfo.Builder()
                .SetTitle(title)
                .SetSubtitle(subtitle)
                .SetAllowedAuthenticators(BiometricManager.Authenticators.BiometricStrong)
                .SetNegativeButtonText("Cancelar")
                .Build();

            prompt.Authenticate(promptInfo);
        });

        return tcs.Task;
    }

    private sealed class PromptCallback : BiometricPrompt.AuthenticationCallback
    {
        private readonly TaskCompletionSource<BiometricResult> _tcs;

        public PromptCallback(TaskCompletionSource<BiometricResult> tcs)
        {
            _tcs = tcs;
        }

        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
        {
            base.OnAuthenticationSucceeded(result);
            _tcs.TrySetResult(new BiometricResult
            {
                Success = true,
                Message = "Autenticacion biometrica exitosa."
            });
        }

        public override void OnAuthenticationError(int errorCode, Java.Lang.ICharSequence? errString)
        {
            var safeError = errString ?? new Java.Lang.String("La autenticacion biometrica fue cancelada.");
            base.OnAuthenticationError(errorCode, safeError);

            _tcs.TrySetResult(new BiometricResult
            {
                Success = false,
                Message = safeError.ToString() ?? "La autenticacion biometrica fue cancelada."
            });
        }

        public override void OnAuthenticationFailed()
        {
            base.OnAuthenticationFailed();
            _tcs.TrySetResult(new BiometricResult
            {
                Success = false,
                Message = "No fue posible validar la biometria."
            });
        }
    }
}
#endif
