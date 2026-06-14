using System.Collections.ObjectModel;
using BakeryPay.Mobile.Models;
using BakeryPay.Mobile.Services;

namespace BakeryPay.Mobile.ViewModels;

public class ProvidersViewModel : BaseViewModel
{
    private readonly ProviderApiService _providerApiService;
    private readonly SessionStorageService _sessionStorageService;
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
    private bool _isCreateModalVisible;

    public ProvidersViewModel(ProviderApiService providerApiService, SessionStorageService sessionStorageService)
    {
        _providerApiService = providerApiService;
        _sessionStorageService = sessionStorageService;
        Title = "Proveedores";
        Providers = new ObservableCollection<ProviderModel>();
        RefreshCommand = new AsyncCommand(LoadAsync);
        CreateProviderCommand = new AsyncCommand(CreateProviderAsync);
        OpenCreateModalCommand = new AsyncCommand(OpenCreateModalAsync);
        CloseCreateModalCommand = new AsyncCommand(CloseCreateModalAsync);
    }

    public ObservableCollection<ProviderModel> Providers { get; }
    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand CreateProviderCommand { get; }
    public AsyncCommand OpenCreateModalCommand { get; }
    public AsyncCommand CloseCreateModalCommand { get; }

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
        get => _isCreateModalVisible;
        set => SetProperty(ref _isCreateModalVisible, value);
    }

    public async Task LoadAsync()
    {
        var session = await _sessionStorageService.GetSessionAsync();
        IsCreateModalVisible = false;
        CanManageProviders = session?.Role is "Administrator" or "Cashier";

        try
        {
            IsBusy = true;
            var items = await _providerApiService.GetProvidersAsync();
            Providers.Clear();

            foreach (var item in items.OrderBy(x => x.BusinessName))
            {
                Providers.Add(item);
            }
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
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetForm()
    {
        Code = string.Empty;
        BusinessName = string.Empty;
        TaxId = string.Empty;
        ContactFirstName = string.Empty;
        ContactLastName = string.Empty;
        ContactIdentificationNumber = string.Empty;
        ContactEmail = string.Empty;
        ContactPhone = string.Empty;
    }

    private Task OpenCreateModalAsync()
    {
        ResetForm();
        Message = string.Empty;
        LastCredentials = string.Empty;
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
