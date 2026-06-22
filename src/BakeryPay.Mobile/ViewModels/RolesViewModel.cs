using System.Collections.ObjectModel;
using BakeryPay.Mobile.Models;
using BakeryPay.Mobile.Services;

namespace BakeryPay.Mobile.ViewModels;

public class RolesViewModel : BaseViewModel
{
    private readonly RoleApiService _roleApiService;
    private RoleModel? _selectedRole;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _message = string.Empty;
    private bool _isFormModalVisible;

    public RolesViewModel(RoleApiService roleApiService)
    {
        _roleApiService = roleApiService;
        Title = "Roles";
        Roles = new ObservableCollection<RoleModel>();
        RefreshCommand = new AsyncCommand(LoadAsync);
        CreateRoleCommand = new AsyncCommand(CreateRoleAsync);
        UpdateRoleCommand = new AsyncCommand(UpdateRoleAsync);
        DeleteRoleCommand = new AsyncCommand(DeleteRoleAsync);
        ClearSelectionCommand = new AsyncCommand(ClearSelectionAsync);
        OpenCreateModalCommand = new AsyncCommand(OpenCreateModalAsync);
        OpenEditModalCommand = new AsyncCommand(OpenEditModalAsync);
        OpenRoleEditModalCommand = new AsyncCommandOfT<RoleModel>(OpenRoleEditModalAsync);
        CloseModalCommand = new AsyncCommand(CloseModalAsync);
    }

    public ObservableCollection<RoleModel> Roles { get; }
    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand CreateRoleCommand { get; }
    public AsyncCommand UpdateRoleCommand { get; }
    public AsyncCommand DeleteRoleCommand { get; }
    public AsyncCommand ClearSelectionCommand { get; }
    public AsyncCommand OpenCreateModalCommand { get; }
    public AsyncCommand OpenEditModalCommand { get; }
    public AsyncCommandOfT<RoleModel> OpenRoleEditModalCommand { get; }
    public AsyncCommand CloseModalCommand { get; }

    public RoleModel? SelectedRole
    {
        get => _selectedRole;
        set
        {
            if (SetProperty(ref _selectedRole, value))
            {
                RaisePropertyChanged(nameof(IsSystemRoleSelected));
                RaisePropertyChanged(nameof(IsRoleSelected));
                RaisePropertyChanged(nameof(SelectedRoleHasUsers));

                if (value is null)
                {
                    return;
                }

                Name = value.Name;
                Description = value.Description;
            }
        }
    }

    public bool IsSystemRoleSelected => SelectedRole?.IsSystemRole == true;
    public bool IsRoleSelected => SelectedRole is not null;
    public bool SelectedRoleHasUsers => SelectedRole?.AssignedUsersCount > 0;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public bool IsFormModalVisible
    {
        get => _isFormModalVisible;
        set => SetProperty(ref _isFormModalVisible, value);
    }

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
        }
        catch (Exception ex)
        {
            Message = $"No fue posible cargar los roles. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateRoleAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description))
        {
            Message = "Completa todos los campos obligatorios.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _roleApiService.CreateRoleAsync(Name, Description);
            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible crear el rol.";
                return;
            }

            Message = response.Message;
            ResetForm();
            IsFormModalVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = $"No fue posible crear el rol. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateRoleAsync()
    {
        if (SelectedRole is null)
        {
            Message = "Selecciona un rol para editar.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description))
        {
            Message = "Completa todos los campos obligatorios.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _roleApiService.UpdateRoleAsync(SelectedRole.Id, Name, Description);
            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible actualizar el rol.";
                return;
            }

            Message = response.Message;
            IsFormModalVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = $"No fue posible actualizar el rol. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteRoleAsync()
    {
        if (SelectedRole is null)
        {
            Message = "Selecciona un rol para eliminar.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _roleApiService.DeleteRoleAsync(SelectedRole.Id);
            if (response?.Success != true)
            {
                Message = response?.Message ?? "No fue posible eliminar el rol.";
                return;
            }

            Message = response.Message;
            ResetForm();
            IsFormModalVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = $"No fue posible eliminar el rol. {ex.Message}";
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
        IsFormModalVisible = false;
        return Task.CompletedTask;
    }

    private void ResetForm()
    {
        SelectedRole = null;
        Name = string.Empty;
        Description = string.Empty;
        RaisePropertyChanged(nameof(IsRoleSelected));
        RaisePropertyChanged(nameof(SelectedRoleHasUsers));
        RaisePropertyChanged(nameof(IsSystemRoleSelected));
    }

    private Task OpenCreateModalAsync()
    {
        ResetForm();
        Message = string.Empty;
        IsFormModalVisible = true;
        return Task.CompletedTask;
    }

    private Task OpenEditModalAsync()
    {
        if (SelectedRole is null)
        {
            Message = "Selecciona un rol para editar.";
            return Task.CompletedTask;
        }

        IsFormModalVisible = true;
        return Task.CompletedTask;
    }

    private Task OpenRoleEditModalAsync(RoleModel? role)
    {
        if (role is null)
        {
            Message = "No fue posible abrir la edicion del rol.";
            return Task.CompletedTask;
        }

        SelectedRole = role;
        Message = string.Empty;
        IsFormModalVisible = true;
        return Task.CompletedTask;
    }

    private Task CloseModalAsync()
    {
        ResetForm();
        IsFormModalVisible = false;
        return Task.CompletedTask;
    }
}
