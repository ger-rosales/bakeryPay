using System.Collections.ObjectModel;
using BakeryPay.Mobile.Models;
using BakeryPay.Mobile.Services;

namespace BakeryPay.Mobile.ViewModels;

public class ProvidersViewModel : BaseViewModel
{
    private readonly ProviderApiService _providerApiService;
    private readonly SessionStorageService _sessionStorageService;
    private ProviderModel? _selectedProvider;
    private string _code = string.Empty;
    private string _businessName = string.Empty;
    private string _taxId = string.Empty;
    private string _contactFirstName = string.Empty;
    private string _contactLastName = string.Empty;
    private string _contactIdentificationNumber = string.Empty;
    private string _contactEmail = string.Empty;
    private string _contactPhone = string.Empty;
    private string _message = string.Empty;
    private bool _canManageProviders;
    private string _lastCredentials = string.Empty;
    private bool _isFormModalVisible;
    private bool _isEditMode;

    public ProvidersViewModel(ProviderApiService providerApiService, SessionStorageService sessionStorageService)
    {
        _providerApiService = providerApiService;
        _sessionStorageService = sessionStorageService;
        Title = "Proveedores";
        Providers = new ObservableCollection<ProviderModel>();
        RefreshCommand = new AsyncCommand(LoadAsync);
        CreateProviderCommand = new AsyncCommand(CreateProviderAsync);
        UpdateProviderCommand = new AsyncCommand(UpdateProviderAsync);
        OpenCreateModalCommand = new AsyncCommand(OpenCreateModalAsync);
        OpenProviderEditModalCommand = new AsyncCommandOfT<ProviderModel>(OpenProviderEditModalAsync);
        CloseCreateModalCommand = new AsyncCommand(CloseCreateModalAsync);
    }

    public ObservableCollection<ProviderModel> Providers { get; }
    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand CreateProviderCommand { get; }
    public AsyncCommand UpdateProviderCommand { get; }
    public AsyncCommand OpenCreateModalCommand { get; }
    public AsyncCommandOfT<ProviderModel> OpenProviderEditModalCommand { get; }
    public AsyncCommand CloseCreateModalCommand { get; }

    public ProviderModel? SelectedProvider
    {
        get => _selectedProvider;
        set => SetProperty(ref _selectedProvider, value);
    }

    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    public string BusinessName
    {
        get => _businessName;
        set => SetProperty(ref _businessName, value);
    }

    public string TaxId
    {
        get => _taxId;
        set => SetProperty(ref _taxId, value);
    }

    public string ContactFirstName
    {
        get => _contactFirstName;
        set => SetProperty(ref _contactFirstName, value);
    }

    public string ContactLastName
    {
        get => _contactLastName;
        set => SetProperty(ref _contactLastName, value);
    }

    public string ContactIdentificationNumber
    {
        get => _contactIdentificationNumber;
        set => SetProperty(ref _contactIdentificationNumber, value);
    }

    public string ContactEmail
    {
        get => _contactEmail;
        set => SetProperty(ref _contactEmail, value);
    }

    public string ContactPhone
    {
        get => _contactPhone;
        set => SetProperty(ref _contactPhone, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public bool CanManageProviders
    {
        get => _canManageProviders;
        set => SetProperty(ref _canManageProviders, value);
    }

    public string LastCredentials
    {
        get => _lastCredentials;
        set => SetProperty(ref _lastCredentials, value);
    }

    public bool IsCreateModalVisible
    {
        get => _isFormModalVisible;
        set => SetProperty(ref _isFormModalVisible, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public string FormTitle => IsEditMode ? "Editar proveedor" : "Crear proveedor";

    public string FormDescription => IsEditMode
        ? "Actualiza los datos del proveedor y del usuario asociado."
        : "Registra los datos del proveedor y el usuario asociado.";

    public string SubmitButtonText => IsEditMode ? "Guardar cambios" : "Crear proveedor";

    public bool IsProviderSelected => SelectedProvider is not null;

    public string SelectedProviderSummary => SelectedProvider is null
        ? string.Empty
        : $"{SelectedProvider.BusinessName} ({SelectedProvider.Code})";

    public bool IsCreateMode => !IsEditMode;

    public async Task LoadAsync()
    {
        var session = await _sessionStorageService.GetSessionAsync();
        IsCreateModalVisible = false;
        IsEditMode = false;
        CanManageProviders = session?.Role is "Administrator" or "Cashier";

        try
        {
            IsBusy = true;
            Message = string.Empty;
            var items = await _providerApiService.GetProvidersAsync();
            Providers.Clear();

            foreach (var item in items.OrderBy(x => x.BusinessName))
            {
                Providers.Add(item);
            }
        }
        catch (Exception ex)
        {
            Message = $"No fue posible cargar los proveedores. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateProviderAsync()
    {
        if (!CanManageProviders)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Code)
            || string.IsNullOrWhiteSpace(BusinessName)
            || string.IsNullOrWhiteSpace(TaxId)
            || string.IsNullOrWhiteSpace(ContactFirstName)
            || string.IsNullOrWhiteSpace(ContactLastName)
            || string.IsNullOrWhiteSpace(ContactIdentificationNumber)
            || string.IsNullOrWhiteSpace(ContactEmail))
        {
            Message = "Completa todos los campos obligatorios.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;
            LastCredentials = string.Empty;

            var response = await _providerApiService.CreateProviderAsync(
                Code,
                BusinessName,
                TaxId,
                ContactFirstName,
                ContactLastName,
                ContactIdentificationNumber,
                ContactEmail,
                ContactPhone);

            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible crear el proveedor.";
                return;
            }

            Message = response.Message;
            LastCredentials = $"Usuario: {response.Data.UserEmail} | Clave temporal: {response.Data.TemporaryPassword}";
            ResetForm();
            IsCreateModalVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = $"No fue posible crear el proveedor. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateProviderAsync()
    {
        if (!CanManageProviders || SelectedProvider is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(TaxId)
            || string.IsNullOrWhiteSpace(ContactFirstName)
            || string.IsNullOrWhiteSpace(ContactLastName)
            || string.IsNullOrWhiteSpace(ContactEmail))
        {
            Message = "Completa todos los campos obligatorios.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;
            LastCredentials = string.Empty;

            var response = await _providerApiService.UpdateProviderAsync(
                SelectedProvider.Id,
                TaxId,
                ContactFirstName,
                ContactLastName,
                ContactEmail,
                ContactPhone);

            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible actualizar el proveedor.";
                return;
            }

            Message = response.Message;
            ResetForm();
            IsCreateModalVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = $"No fue posible actualizar el proveedor. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetForm()
    {
        SelectedProvider = null;
        Code = string.Empty;
        BusinessName = string.Empty;
        TaxId = string.Empty;
        ContactFirstName = string.Empty;
        ContactLastName = string.Empty;
        ContactIdentificationNumber = string.Empty;
        ContactEmail = string.Empty;
        ContactPhone = string.Empty;
        IsEditMode = false;
        RaisePropertyChanged(nameof(FormTitle));
        RaisePropertyChanged(nameof(FormDescription));
        RaisePropertyChanged(nameof(SubmitButtonText));
        RaisePropertyChanged(nameof(IsProviderSelected));
        RaisePropertyChanged(nameof(SelectedProviderSummary));
        RaisePropertyChanged(nameof(IsCreateMode));
    }

    private Task OpenCreateModalAsync()
    {
        ResetForm();
        Message = string.Empty;
        LastCredentials = string.Empty;
        IsCreateModalVisible = true;
        return Task.CompletedTask;
    }

    private Task OpenProviderEditModalAsync(ProviderModel? provider)
    {
        if (provider is null)
        {
            Message = "No fue posible abrir la edicion del proveedor.";
            return Task.CompletedTask;
        }

        SelectedProvider = provider;
        Code = provider.Code;
        BusinessName = provider.BusinessName;
        TaxId = provider.TaxId;
        ContactFirstName = provider.ContactFirstName;
        ContactLastName = provider.ContactLastName;
        ContactIdentificationNumber = provider.ContactIdentificationNumber;
        ContactEmail = string.IsNullOrWhiteSpace(provider.UserEmail) ? provider.Email : provider.UserEmail;
        ContactPhone = provider.Phone;
        LastCredentials = string.Empty;
        Message = string.Empty;
        IsEditMode = true;
        RaisePropertyChanged(nameof(FormTitle));
        RaisePropertyChanged(nameof(FormDescription));
        RaisePropertyChanged(nameof(SubmitButtonText));
        RaisePropertyChanged(nameof(IsProviderSelected));
        RaisePropertyChanged(nameof(SelectedProviderSummary));
        RaisePropertyChanged(nameof(IsCreateMode));
        IsCreateModalVisible = true;
        return Task.CompletedTask;
    }

    private Task CloseCreateModalAsync()
    {
        ResetForm();
        IsCreateModalVisible = false;
        return Task.CompletedTask;
    }
}
