using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.Services.Biometric;
using BakeryPay.Mobile.Views;

namespace BakeryPay.Mobile.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly AuthApiService _authApiService;
    private readonly SessionStorageService _sessionStorageService;
    private readonly DeviceInstallationService _deviceInstallationService;
    private readonly IBiometricService _biometricService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _message = string.Empty;

    public LoginViewModel(
        AuthApiService authApiService,
        SessionStorageService sessionStorageService,
        DeviceInstallationService deviceInstallationService,
        IBiometricService biometricService)
    {
        _authApiService = authApiService;
        _sessionStorageService = sessionStorageService;
        _deviceInstallationService = deviceInstallationService;
        _biometricService = biometricService;

        Title = "Ingreso seguro";
        LoginCommand = new AsyncCommand(LoginAsync);
        BiometricLoginCommand = new AsyncCommand(BiometricLoginAsync);
        OpenCashierRegistrationCommand = new AsyncCommand(OpenCashierRegistrationAsync);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public AsyncCommand LoginCommand { get; }
    public AsyncCommand BiometricLoginCommand { get; }
    public AsyncCommand OpenCashierRegistrationCommand { get; }

    private async Task LoginAsync()
    {
        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _authApiService.LoginAsync(Email, Password);
            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible iniciar sesion.";
                return;
            }

            await _sessionStorageService.SaveSessionAsync(response.Data);
            if (Shell.Current is AppShell shell)
            {
                await shell.RefreshNavigationAsync();
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task BiometricLoginAsync()
    {
        try
        {
            IsBusy = true;
            Message = string.Empty;

            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Ingresa tu correo para validar el usuario biometrico.";
                return;
            }

            var availability = await _biometricService.GetAvailabilityAsync();
            if (!availability.IsAvailable)
            {
                Message = "La biometria no esta disponible en este dispositivo.";
                return;
            }

            var biometricResult = await _biometricService.AuthenticateAsync("BakeryPay", "Confirma tu identidad");
            if (!biometricResult.Success)
            {
                Message = biometricResult.Message;
                return;
            }

            var deviceId = await _deviceInstallationService.GetOrCreateDeviceIdAsync();
            var response = await _authApiService.BiometricLoginAsync(Email, deviceId, (int)availability.BiometricType);
            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible iniciar sesion con biometria.";
                return;
            }

            await _sessionStorageService.SaveSessionAsync(response.Data);
            if (Shell.Current is AppShell shell)
            {
                await shell.RefreshNavigationAsync();
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task OpenCashierRegistrationAsync() => Shell.Current.GoToAsync("//register-cashier");
}
