using BakeryPay.Mobile.Services;

namespace BakeryPay.Mobile.ViewModels;

public class ChangePasswordViewModel : BaseViewModel
{
    private readonly AuthApiService _authApiService;
    private readonly SessionStorageService _sessionStorageService;
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmNewPassword = string.Empty;
    private string _message = string.Empty;

    public ChangePasswordViewModel(AuthApiService authApiService, SessionStorageService sessionStorageService)
    {
        _authApiService = authApiService;
        _sessionStorageService = sessionStorageService;
        Title = "Cambio de clave";
        ChangePasswordCommand = new AsyncCommand(ChangePasswordAsync);
    }

    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmNewPassword
    {
        get => _confirmNewPassword;
        set => SetProperty(ref _confirmNewPassword, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public AsyncCommand ChangePasswordCommand { get; }

    private async Task ChangePasswordAsync()
    {
        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _authApiService.ChangePasswordAsync(
                CurrentPassword,
                NewPassword,
                ConfirmNewPassword);

            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible cambiar la clave.";
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
}
