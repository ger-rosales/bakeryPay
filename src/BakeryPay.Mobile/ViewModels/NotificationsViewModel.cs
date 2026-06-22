using System.Collections.ObjectModel;
using BakeryPay.Mobile.Models;
using BakeryPay.Mobile.Services;

namespace BakeryPay.Mobile.ViewModels;

public class NotificationsViewModel : BaseViewModel
{
    private readonly NotificationApiService _notificationApiService;
    private readonly ProviderApiService _providerApiService;
    private readonly SessionStorageService _sessionStorageService;
    private ProviderModel? _selectedProvider;
    private string _titleText = string.Empty;
    private string _messageText = string.Empty;
    private SelectionOptionModel? _selectedType;
    private string _message = string.Empty;
    private bool _isInternalRole;
    private bool _isCreateModalVisible;

    public NotificationsViewModel(
        NotificationApiService notificationApiService,
        ProviderApiService providerApiService,
        SessionStorageService sessionStorageService)
    {
        _notificationApiService = notificationApiService;
        _providerApiService = providerApiService;
        _sessionStorageService = sessionStorageService;
        Title = "Notificaciones";
        Notifications = new ObservableCollection<NotificationModel>();
        Providers = new ObservableCollection<ProviderModel>();
        NotificationTypes = new ObservableCollection<SelectionOptionModel>(new[]
        {
            new SelectionOptionModel { Value = "General", Label = "General" },
            new SelectionOptionModel { Value = "PaymentRegistered", Label = "Pago registrado" },
            new SelectionOptionModel { Value = "ReceiptUploaded", Label = "Comprobante cargado" }
        });
        RefreshCommand = new AsyncCommand(LoadAsync);
        MarkAsReadCommand = new AsyncCommandOfT<NotificationModel>(MarkAsReadAsync);
        CreateNotificationCommand = new AsyncCommand(CreateNotificationAsync);
        OpenCreateModalCommand = new AsyncCommand(OpenCreateModalAsync);
        CloseCreateModalCommand = new AsyncCommand(CloseCreateModalAsync);
        SelectedType = NotificationTypes.FirstOrDefault(x => x.Value == "General");
    }

    public ObservableCollection<NotificationModel> Notifications { get; }
    public ObservableCollection<ProviderModel> Providers { get; }
    public ObservableCollection<SelectionOptionModel> NotificationTypes { get; }
    public AsyncCommand RefreshCommand { get; }
    public AsyncCommandOfT<NotificationModel> MarkAsReadCommand { get; }
    public AsyncCommand CreateNotificationCommand { get; }
    public AsyncCommand OpenCreateModalCommand { get; }
    public AsyncCommand CloseCreateModalCommand { get; }

    public ProviderModel? SelectedProvider
    {
        get => _selectedProvider;
        set => SetProperty(ref _selectedProvider, value);
    }

    public string TitleText
    {
        get => _titleText;
        set => SetProperty(ref _titleText, value);
    }

    public string MessageText
    {
        get => _messageText;
        set => SetProperty(ref _messageText, value);
    }

    public SelectionOptionModel? SelectedType
    {
        get => _selectedType;
        set => SetProperty(ref _selectedType, value);
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

            Guid? providerId;
            if (IsInternalRole)
            {
                if (Providers.Count == 0)
                {
                    var providers = await _providerApiService.GetProvidersAsync();
                    Providers.Clear();
                    foreach (var item in providers.OrderBy(x => x.BusinessName))
                    {
                        Providers.Add(item);
                    }
                }

                SelectedProvider ??= Providers.FirstOrDefault();
                SelectedType ??= NotificationTypes.FirstOrDefault(x => x.Value == "General");
                providerId = SelectedProvider?.Id;
            }
            else
            {
                providerId = session.ProviderId;
            }

            if (providerId is null)
            {
                return;
            }

            var items = await _notificationApiService.GetByProviderAsync(providerId.Value);
            Notifications.Clear();

            foreach (var item in items.OrderByDescending(x => x.SentAtUtc))
            {
                Notifications.Add(item);
            }
        }
        catch (Exception ex)
        {
            Message = $"No fue posible cargar las notificaciones. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateNotificationAsync()
    {
        if (!IsInternalRole || SelectedProvider is null)
        {
            Message = "Selecciona un proveedor.";
            return;
        }

        if (string.IsNullOrWhiteSpace(TitleText) || string.IsNullOrWhiteSpace(MessageText))
        {
            Message = "Completa todos los campos obligatorios.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _notificationApiService.CreateAsync(
                SelectedProvider.Id,
                TitleText,
                MessageText,
                SelectedType?.Value ?? "General");

            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible crear la notificacion.";
                return;
            }

            TitleText = string.Empty;
            MessageText = string.Empty;
            SelectedType = NotificationTypes.FirstOrDefault(x => x.Value == "General");
            IsCreateModalVisible = false;
            Message = response.Message;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = $"No fue posible crear la notificacion. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MarkAsReadAsync(NotificationModel? notification)
    {
        if (notification is null || notification.IsRead)
        {
            return;
        }

        try
        {
            Message = string.Empty;
            await _notificationApiService.MarkAsReadAsync(notification.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Message = $"No fue posible actualizar la notificacion. {ex.Message}";
        }
    }

    private Task OpenCreateModalAsync()
    {
        TitleText = string.Empty;
        MessageText = string.Empty;
        SelectedType = NotificationTypes.FirstOrDefault(x => x.Value == "General");
        Message = string.Empty;
        IsCreateModalVisible = true;
        return Task.CompletedTask;
    }

    private Task CloseCreateModalAsync()
    {
        TitleText = string.Empty;
        MessageText = string.Empty;
        SelectedType = NotificationTypes.FirstOrDefault(x => x.Value == "General");
        IsCreateModalVisible = false;
        return Task.CompletedTask;
    }
}
