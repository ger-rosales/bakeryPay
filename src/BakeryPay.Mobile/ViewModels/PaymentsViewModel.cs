using System.Collections.ObjectModel;
using BakeryPay.Mobile.Models;
using BakeryPay.Mobile.Services;

namespace BakeryPay.Mobile.ViewModels;

public class PaymentsViewModel : BaseViewModel
{
    private readonly PaymentApiService _paymentApiService;
    private readonly ProviderApiService _providerApiService;
    private readonly SessionStorageService _sessionStorageService;
    private ProviderModel? _selectedProvider;
    private decimal _amount;
    private string _currency = "USD";
    private DateTime _paymentDateUtc = DateTime.Today;
    private string _referenceNumber = string.Empty;
    private string _description = string.Empty;
    private SelectionOptionModel? _selectedStatus;
    private string _message = string.Empty;
    private bool _isInternalRole;
    private bool _isCreateModalVisible;

    public PaymentsViewModel(
        PaymentApiService paymentApiService,
        ProviderApiService providerApiService,
        SessionStorageService sessionStorageService)
    {
        _paymentApiService = paymentApiService;
        _providerApiService = providerApiService;
        _sessionStorageService = sessionStorageService;
        Title = "Pagos";
        Payments = new ObservableCollection<PaymentModel>();
        Providers = new ObservableCollection<ProviderModel>();
        PaymentStatuses = new ObservableCollection<SelectionOptionModel>(new[]
        {
            new SelectionOptionModel { Value = "Pending", Label = "Pendiente" },
            new SelectionOptionModel { Value = "Paid", Label = "Pagado" },
            new SelectionOptionModel { Value = "Rejected", Label = "Rechazado" }
        });
        RefreshCommand = new AsyncCommand(LoadAsync);
        OpenDetailCommand = new AsyncCommandOfT<PaymentModel>(OpenDetailAsync);
        CreatePaymentCommand = new AsyncCommand(CreatePaymentAsync);
        OpenCreateModalCommand = new AsyncCommand(OpenCreateModalAsync);
        CloseCreateModalCommand = new AsyncCommand(CloseCreateModalAsync);
        SelectedStatus = PaymentStatuses.FirstOrDefault(x => x.Value == "Paid");
    }

    public ObservableCollection<PaymentModel> Payments { get; }
    public ObservableCollection<ProviderModel> Providers { get; }
    public ObservableCollection<SelectionOptionModel> PaymentStatuses { get; }
    public AsyncCommand RefreshCommand { get; }
    public AsyncCommandOfT<PaymentModel> OpenDetailCommand { get; }
    public AsyncCommand CreatePaymentCommand { get; }
    public AsyncCommand OpenCreateModalCommand { get; }
    public AsyncCommand CloseCreateModalCommand { get; }

    public ProviderModel? SelectedProvider
    {
        get => _selectedProvider;
        set => SetProperty(ref _selectedProvider, value);
    }

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public string Currency
    {
        get => _currency;
        set => SetProperty(ref _currency, value);
    }

    public DateTime PaymentDateUtc
    {
        get => _paymentDateUtc;
        set => SetProperty(ref _paymentDateUtc, value);
    }

    public string ReferenceNumber
    {
        get => _referenceNumber;
        set => SetProperty(ref _referenceNumber, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public SelectionOptionModel? SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public bool IsInternalRole
    {
        get => _isInternalRole;
        set => SetProperty(ref _isInternalRole, value);
    }

    public bool IsCreateModalVisible
    {
        get => _isCreateModalVisible;
        set => SetProperty(ref _isCreateModalVisible, value);
    }

    public async Task LoadAsync()
    {
        var session = await _sessionStorageService.GetSessionAsync();
        if (session is null)
        {
            return;
        }

        IsCreateModalVisible = false;
        IsInternalRole = session.Role is "Administrator" or "Cashier";

        try
        {
            IsBusy = true;
            Message = string.Empty;

            if (IsInternalRole)
            {
                var providers = await _providerApiService.GetProvidersAsync();
                Providers.Clear();
                foreach (var provider in providers)
                {
                    Providers.Add(provider);
                }

                SelectedProvider ??= Providers.FirstOrDefault();
                SelectedStatus ??= PaymentStatuses.FirstOrDefault(x => x.Value == "Paid");
                var allPayments = await _paymentApiService.GetAllPaymentsAsync();
                ReplacePayments(allPayments);
                return;
            }

            if (session.ProviderId is null)
            {
                return;
            }

            var items = await _paymentApiService.GetPaymentsByProviderAsync(session.ProviderId.Value);
            ReplacePayments(items);
        }
        catch (Exception ex)
        {
            Message = $"No fue posible cargar los pagos. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreatePaymentAsync()
    {
        if (!IsInternalRole)
        {
            return;
        }

        if (SelectedProvider is null)
        {
            Message = "Selecciona un proveedor.";
            return;
        }

        if (Amount <= 0 || string.IsNullOrWhiteSpace(ReferenceNumber))
        {
            Message = "Completa todos los campos obligatorios.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Currency))
        {
            Message = "Completa todos los campos obligatorios.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _paymentApiService.CreatePaymentAsync(
                SelectedProvider.Id,
                Amount,
                Currency,
                PaymentDateUtc,
                ReferenceNumber,
                Description,
                SelectedStatus?.Value ?? "Paid");

            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible registrar el pago.";
                return;
            }

            ResetForm();
            IsCreateModalVisible = false;
            Message = response.Message;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = $"No fue posible registrar el pago. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ReplacePayments(IEnumerable<PaymentModel> items)
    {
        Payments.Clear();
        foreach (var item in items.OrderByDescending(x => x.PaymentDateUtc))
        {
            Payments.Add(item);
        }
    }

    private void ResetForm()
    {
        Amount = 0;
        Currency = "USD";
        PaymentDateUtc = DateTime.Today;
        ReferenceNumber = string.Empty;
        Description = string.Empty;
        SelectedStatus = PaymentStatuses.FirstOrDefault(x => x.Value == "Paid");
    }

    private Task OpenCreateModalAsync()
    {
        ResetForm();
        Message = string.Empty;
        IsCreateModalVisible = true;
        return Task.CompletedTask;
    }

    private Task CloseCreateModalAsync()
    {
        ResetForm();
        IsCreateModalVisible = false;
        return Task.CompletedTask;
    }

    private static async Task OpenDetailAsync(PaymentModel? payment)
    {
        if (payment is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(Views.PaymentDetailPage)}?paymentId={payment.Id}");
    }
}
