using BakeryPay.Mobile.Services;

namespace BakeryPay.Mobile.ViewModels;

public class RegisterCashierViewModel : BaseViewModel
{
    private readonly AuthApiService _authApiService;
    private readonly SessionStorageService _sessionStorageService;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _identificationNumber = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _message = string.Empty;
    private bool _acceptPolicies;

    public RegisterCashierViewModel(AuthApiService authApiService, SessionStorageService sessionStorageService)
    {
        _authApiService = authApiService;
        _sessionStorageService = sessionStorageService;
        Title = "Registro de cajera";
        RegisterCommand = new AsyncCommand(RegisterAsync);
        BackToLoginCommand = new AsyncCommand(BackToLoginAsync);
    }

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string IdentificationNumber
    {
        get => _identificationNumber;
        set => SetProperty(ref _identificationNumber, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public bool AcceptPolicies
    {
        get => _acceptPolicies;
        set => SetProperty(ref _acceptPolicies, value);
    }

    public AsyncCommand RegisterCommand { get; }
    public AsyncCommand BackToLoginCommand { get; }

    private async Task RegisterAsync()
    {
        try
        {
            IsBusy = true;
            Message = string.Empty;

            if (string.IsNullOrWhiteSpace(FirstName)
                || string.IsNullOrWhiteSpace(LastName)
                || string.IsNullOrWhiteSpace(IdentificationNumber)
                || string.IsNullOrWhiteSpace(Email)
                || string.IsNullOrWhiteSpace(Phone)
                || string.IsNullOrWhiteSpace(Password)
                || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                Message = "Completa todos los campos obligatorios.";
                return;
            }

            if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
            {
                Message = "La confirmacion de la clave no coincide.";
                return;
            }

            if (!AcceptPolicies)
            {
                Message = "Debes aceptar las politicas para continuar.";
                return;
            }

            var response = await _authApiService.RegisterCashierAsync(
                FirstName,
                LastName,
                IdentificationNumber,
                Email,
                Phone,
                Password,
                ConfirmPassword,
                AcceptPolicies);

            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible completar el registro.";
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

    private Task BackToLoginAsync() => Shell.Current.GoToAsync("//login");
}
