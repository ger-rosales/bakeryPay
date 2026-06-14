using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.Services.Biometric;

namespace BakeryPay.Mobile.ViewModels;

public class SecurityViewModel : BaseViewModel
{
    private readonly AuthApiService _authApiService;
    private readonly SessionStorageService _sessionStorageService;
    private readonly DeviceInstallationService _deviceInstallationService;
    private readonly IBiometricService _biometricService;
    private string _email = string.Empty;
    private string _fullName = string.Empty;
    private string _message = string.Empty;
    private string _biometricStatus = "Biometria no registrada";
    private bool _isBiometricRegistered;

    public SecurityViewModel(
        AuthApiService authApiService,
        SessionStorageService sessionStorageService,
        DeviceInstallationService deviceInstallationService,
        IBiometricService biometricService)
    {
        _authApiService = authApiService;
        _sessionStorageService = sessionStorageService;
        _deviceInstallationService = deviceInstallationService;
        _biometricService = biometricService;

        Title = "Seguridad";
        RegisterBiometricCommand = new AsyncCommand(RegisterBiometricAsync);
        OpenChangePasswordCommand = new AsyncCommand(OpenChangePasswordAsync);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string BiometricStatus
    {
        get => _biometricStatus;
        set => SetProperty(ref _biometricStatus, value);
    }

    public bool IsBiometricRegistered
    {
        get => _isBiometricRegistered;
        set => SetProperty(ref _isBiometricRegistered, value);
    }

    public AsyncCommand RegisterBiometricCommand { get; }
    public AsyncCommand OpenChangePasswordCommand { get; }

    public async Task LoadAsync()
    {
        var session = await _sessionStorageService.GetSessionAsync();
        if (session is null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        Email = session.Email;
        FullName = session.FullName;
        IsBiometricRegistered = session.BiometricEnabled;
        BiometricStatus = session.BiometricEnabled
            ? "Biometria registrada para este dispositivo"
            : "Biometria no registrada";
    }

    private async Task RegisterBiometricAsync()
    {
        try
        {
            IsBusy = true;
            Message = string.Empty;

            var availability = await _biometricService.GetAvailabilityAsync();
            if (!availability.IsAvailable)
            {
                Message = "La biometria no esta disponible en este dispositivo.";
                return;
            }

            var biometricResult = await _biometricService.AuthenticateAsync("BakeryPay", "Autoriza el registro biometrico");
            if (!biometricResult.Success)
            {
                Message = biometricResult.Message;
                return;
            }

            var deviceId = await _deviceInstallationService.GetOrCreateDeviceIdAsync();
            var response = await _authApiService.RegisterBiometricAsync(
                deviceId,
                DeviceInfo.Current.Model,
                DeviceInfo.Current.Platform.ToString(),
                (int)availability.BiometricType);

            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible registrar la biometria.";
                return;
            }

            await _sessionStorageService.SaveSessionAsync(response.Data);
            IsBiometricRegistered = true;
            BiometricStatus = $"{availability.DisplayName} registrada correctamente";
            Message = "Biometria habilitada para futuros ingresos.";
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

    private Task OpenChangePasswordAsync() => Shell.Current.GoToAsync("//change-password");
}
