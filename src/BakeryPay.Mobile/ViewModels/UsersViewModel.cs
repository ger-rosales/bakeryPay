using System.Collections.ObjectModel;
using BakeryPay.Mobile.Models;
using BakeryPay.Mobile.Services;

namespace BakeryPay.Mobile.ViewModels;

public class UsersViewModel : BaseViewModel
{
    private const string AdministratorRoleName = "Administrator";
    private const string ProviderRoleName = "Provider";
    private readonly UserApiService _userApiService;
    private readonly RoleApiService _roleApiService;
    private UserModel? _selectedUser;
    private RoleModel? _selectedRole;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _identificationNumber = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string _password = string.Empty;
    private string _message = string.Empty;
    private string _lastTemporaryPassword = string.Empty;
    private bool _isFormModalVisible;

    public UsersViewModel(UserApiService userApiService, RoleApiService roleApiService)
    {
        _userApiService = userApiService;
        _roleApiService = roleApiService;
        Title = "Usuarios";
        Users = new ObservableCollection<UserModel>();
        Roles = new ObservableCollection<RoleModel>();
        RefreshCommand = new AsyncCommand(LoadAsync);
        CreateUserCommand = new AsyncCommand(CreateUserAsync);
        UpdateUserCommand = new AsyncCommand(UpdateUserAsync);
        ToggleStatusCommand = new AsyncCommand(ToggleStatusAsync);
        ResetPasswordCommand = new AsyncCommand(ResetPasswordAsync);
        ClearSelectionCommand = new AsyncCommand(ClearSelectionAsync);
        OpenCreateModalCommand = new AsyncCommand(OpenCreateModalAsync);
        OpenEditModalCommand = new AsyncCommand(OpenEditModalAsync);
        OpenUserEditModalCommand = new AsyncCommandOfT<UserModel>(OpenUserEditModalAsync);
        CloseModalCommand = new AsyncCommand(CloseModalAsync);
    }

    public ObservableCollection<UserModel> Users { get; }
    public ObservableCollection<RoleModel> Roles { get; }
    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand CreateUserCommand { get; }
    public AsyncCommand UpdateUserCommand { get; }
    public AsyncCommand ToggleStatusCommand { get; }
    public AsyncCommand ResetPasswordCommand { get; }
    public AsyncCommand ClearSelectionCommand { get; }
    public AsyncCommand OpenCreateModalCommand { get; }
    public AsyncCommand OpenEditModalCommand { get; }
    public AsyncCommandOfT<UserModel> OpenUserEditModalCommand { get; }
    public AsyncCommand CloseModalCommand { get; }

    public UserModel? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                RaiseComputedProperties();

                if (value is null)
                {
                    return;
                }

                FirstName = value.FirstName;
                LastName = value.LastName;
                IdentificationNumber = value.IdentificationNumber;
                Email = value.Email;
                Phone = value.Phone;
                SelectedRole = Roles.FirstOrDefault(x => x.Id == value.RoleId);
                Password = string.Empty;
                LastTemporaryPassword = string.Empty;
            }
        }
    }

    public RoleModel? SelectedRole
    {
        get => _selectedRole;
        set => SetProperty(ref _selectedRole, value);
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

    public string IdentificationNumber
    {
        get => _identificationNumber;
        set => SetProperty(ref _identificationNumber, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
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

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string LastTemporaryPassword
    {
        get => _lastTemporaryPassword;
        set => SetProperty(ref _lastTemporaryPassword, value);
    }

    public bool IsFormModalVisible
    {
        get => _isFormModalVisible;
        set => SetProperty(ref _isFormModalVisible, value);
    }

    public bool IsUserSelected => SelectedUser is not null;

    public bool SelectedUserIsAdministrator => string.Equals(SelectedUser?.RoleName, AdministratorRoleName, StringComparison.OrdinalIgnoreCase);

    public bool SelectedUserIsProvider => string.Equals(SelectedUser?.RoleName, ProviderRoleName, StringComparison.OrdinalIgnoreCase);

    public string SelectedUserStatusLabel => SelectedUser?.IsActive == true ? "Activo" : "Inactivo";

    public string ToggleStatusButtonText => SelectedUser?.IsActive == true ? "Desactivar usuario seleccionado" : "Activar usuario seleccionado";

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            IsFormModalVisible = false;
            Message = string.Empty;
            var roles = await _roleApiService.GetRolesAsync();
            Roles.Clear();
            foreach (var role in roles.OrderBy(x => x.Name))
            {
                Roles.Add(role);
            }

            var users = await _userApiService.GetUsersAsync();
            Users.Clear();
            foreach (var user in users.OrderBy(x => x.RoleName).ThenBy(x => x.FirstName))
            {
                Users.Add(user);
            }
        }
        catch (Exception ex)
        {
            Message = $"No fue posible cargar los usuarios. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateUserAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName)
            || string.IsNullOrWhiteSpace(LastName)
            || string.IsNullOrWhiteSpace(IdentificationNumber)
            || string.IsNullOrWhiteSpace(Email)
            || string.IsNullOrWhiteSpace(Phone)
            || string.IsNullOrWhiteSpace(Password))
        {
            Message = "Completa todos los campos obligatorios.";
            return;
        }

        if (SelectedRole is null)
        {
            Message = "Selecciona un rol.";
            return;
        }

        if (SelectedRole.Name == ProviderRoleName)
        {
            Message = "Los usuarios proveedor se crean desde el modulo de proveedores.";
            return;
        }

        if (SelectedRole.Name == AdministratorRoleName)
        {
            Message = "El administrador principal se configura internamente y no se crea desde este modulo.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _userApiService.CreateUserAsync(
                FirstName,
                LastName,
                IdentificationNumber,
                Email,
                Password,
                Phone,
                SelectedRole.Id);

            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible crear el usuario.";
                return;
            }

            Message = response.Message;
            LastTemporaryPassword = "El usuario debera cambiar su clave en el primer ingreso.";
            ResetForm();
            IsFormModalVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = $"No fue posible crear el usuario. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateUserAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName)
            || string.IsNullOrWhiteSpace(LastName)
            || string.IsNullOrWhiteSpace(IdentificationNumber)
            || string.IsNullOrWhiteSpace(Email)
            || string.IsNullOrWhiteSpace(Phone))
        {
            Message = "Completa todos los campos obligatorios.";
            return;
        }

        if (SelectedUser is null || SelectedRole is null)
        {
            Message = "Selecciona un usuario y un rol.";
            return;
        }

        if (SelectedRole.Name == ProviderRoleName)
        {
            Message = "Los usuarios proveedor se administran desde el modulo de proveedores.";
            return;
        }

        if (SelectedRole.Name == AdministratorRoleName && SelectedUser.RoleName != AdministratorRoleName)
        {
            Message = "No puedes promover usuarios al rol Administrator desde este modulo.";
            return;
        }

        if (SelectedRole.Name != AdministratorRoleName && SelectedUser.RoleName == AdministratorRoleName)
        {
            Message = "No puedes cambiar el rol del administrador principal desde este modulo.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _userApiService.UpdateUserAsync(
                SelectedUser.Id,
                FirstName,
                LastName,
                IdentificationNumber,
                Email,
                Phone,
                SelectedRole.Id);

            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible actualizar el usuario.";
                return;
            }

            Message = response.Message;
            IsFormModalVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = $"No fue posible actualizar el usuario. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ToggleStatusAsync()
    {
        if (SelectedUser is null)
        {
            Message = "Selecciona un usuario.";
            return;
        }

        if (SelectedUserIsAdministrator)
        {
            Message = "El administrador principal no puede activarse ni desactivarse desde este modulo.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;
            LastTemporaryPassword = string.Empty;

            var response = await _userApiService.SetUserStatusAsync(SelectedUser.Id, !SelectedUser.IsActive);
            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible cambiar el estado del usuario.";
                return;
            }

            Message = response.Message;
            await LoadAsync();
            SelectedUser = Users.FirstOrDefault(x => x.Id == response.Data.Id);
        }
        catch (Exception ex)
        {
            Message = $"No fue posible cambiar el estado del usuario. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResetPasswordAsync()
    {
        if (SelectedUser is null)
        {
            Message = "Selecciona un usuario.";
            return;
        }

        if (SelectedUserIsAdministrator)
        {
            Message = "La clave del administrador principal no se resetea desde este modulo.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _userApiService.ResetPasswordAsync(SelectedUser.Id);
            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible resetear la clave.";
                return;
            }

            Message = response.Message;
            LastTemporaryPassword = $"Clave temporal para {response.Data.Email}: {response.Data.TemporaryPassword}";
            await LoadAsync();
            SelectedUser = Users.FirstOrDefault(x => x.Id == response.Data.UserId);
        }
        catch (Exception ex)
        {
            Message = $"No fue posible resetear la clave. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task ClearSelectionAsync()
    {
        ResetForm();
        Message = string.Empty;
        LastTemporaryPassword = string.Empty;
        IsFormModalVisible = false;
        return Task.CompletedTask;
    }

    private void ResetForm()
    {
        SelectedUser = null;
        FirstName = string.Empty;
        LastName = string.Empty;
        IdentificationNumber = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Password = string.Empty;
        SelectedRole = null;
        RaiseComputedProperties();
    }

    private Task OpenCreateModalAsync()
    {
        ResetForm();
        Message = string.Empty;
        LastTemporaryPassword = string.Empty;
        IsFormModalVisible = true;
        return Task.CompletedTask;
    }

    private Task OpenEditModalAsync()
    {
        if (SelectedUser is null)
        {
            Message = "Selecciona un usuario para editar.";
            return Task.CompletedTask;
        }

        IsFormModalVisible = true;
        return Task.CompletedTask;
    }

    private Task OpenUserEditModalAsync(UserModel? user)
    {
        if (user is null)
        {
            Message = "No fue posible abrir la edicion del usuario.";
            return Task.CompletedTask;
        }

        SelectedUser = user;
        Message = string.Empty;
        LastTemporaryPassword = string.Empty;
        IsFormModalVisible = true;
        return Task.CompletedTask;
    }

    private Task CloseModalAsync()
    {
        ResetForm();
        IsFormModalVisible = false;
        return Task.CompletedTask;
    }

    private void RaiseComputedProperties()
    {
        RaisePropertyChanged(nameof(IsUserSelected));
        RaisePropertyChanged(nameof(SelectedUserIsAdministrator));
        RaisePropertyChanged(nameof(SelectedUserIsProvider));
        RaisePropertyChanged(nameof(SelectedUserStatusLabel));
        RaisePropertyChanged(nameof(ToggleStatusButtonText));
    }
}
