using System.Collections.ObjectModel;
using BakeryPay.Mobile.Models;
using BakeryPay.Mobile.Services;

namespace BakeryPay.Mobile.ViewModels;

public class PaymentDetailViewModel : BaseViewModel
{
    private readonly PaymentApiService _paymentApiService;
    private readonly SessionStorageService _sessionStorageService;
    private PaymentModel? _payment;
    private string _message = string.Empty;
    private bool _canUploadReceipts;
    private bool _canManagePayments;
    private SelectionOptionModel? _selectedStatus;
    private bool _isStatusModalVisible;
    private bool _isReceiptModalVisible;

    public PaymentDetailViewModel(PaymentApiService paymentApiService, SessionStorageService sessionStorageService)
    {
        _paymentApiService = paymentApiService;
        _sessionStorageService = sessionStorageService;
        Title = "Detalle del pago";
        Receipts = new ObservableCollection<ReceiptModel>();
        PaymentStatuses = new ObservableCollection<SelectionOptionModel>(new[]
        {
            new SelectionOptionModel { Value = "Pending", Label = "Pendiente" },
            new SelectionOptionModel { Value = "Paid", Label = "Pagado" },
            new SelectionOptionModel { Value = "Rejected", Label = "Rechazado" }
        });
        DownloadReceiptCommand = new AsyncCommandOfT<ReceiptModel>(DownloadReceiptAsync);
        UploadReceiptCommand = new AsyncCommand(UploadReceiptAsync);
        UpdateStatusCommand = new AsyncCommand(UpdateStatusAsync);
        OpenStatusModalCommand = new AsyncCommand(OpenStatusModalAsync);
        CloseStatusModalCommand = new AsyncCommand(CloseStatusModalAsync);
        OpenReceiptModalCommand = new AsyncCommand(OpenReceiptModalAsync);
        CloseReceiptModalCommand = new AsyncCommand(CloseReceiptModalAsync);
    }

    public PaymentModel? Payment
    {
        get => _payment;
        set
        {
            if (SetProperty(ref _payment, value) && value is not null)
            {
                SelectedStatus = PaymentStatuses.FirstOrDefault(x => x.Value == value.Status)
                    ?? PaymentStatuses.FirstOrDefault();
            }
        }
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public bool CanUploadReceipts
    {
        get => _canUploadReceipts;
        set => SetProperty(ref _canUploadReceipts, value);
    }

    public bool CanManagePayments
    {
        get => _canManagePayments;
        set => SetProperty(ref _canManagePayments, value);
    }

    public SelectionOptionModel? SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    public bool IsStatusModalVisible
    {
        get => _isStatusModalVisible;
        set => SetProperty(ref _isStatusModalVisible, value);
    }

    public bool IsReceiptModalVisible
    {
        get => _isReceiptModalVisible;
        set => SetProperty(ref _isReceiptModalVisible, value);
    }

    public ObservableCollection<ReceiptModel> Receipts { get; }
    public ObservableCollection<SelectionOptionModel> PaymentStatuses { get; }
    public AsyncCommandOfT<ReceiptModel> DownloadReceiptCommand { get; }
    public AsyncCommand UploadReceiptCommand { get; }
    public AsyncCommand UpdateStatusCommand { get; }
    public AsyncCommand OpenStatusModalCommand { get; }
    public AsyncCommand CloseStatusModalCommand { get; }
    public AsyncCommand OpenReceiptModalCommand { get; }
    public AsyncCommand CloseReceiptModalCommand { get; }

    public async Task LoadAsync(Guid paymentId)
    {
        try
        {
            IsBusy = true;
            Message = string.Empty;
            IsStatusModalVisible = false;
            IsReceiptModalVisible = false;

            var session = await _sessionStorageService.GetSessionAsync();
            CanUploadReceipts = session?.Role is "Administrator" or "Cashier";
            CanManagePayments = CanUploadReceipts;

            Payment = await _paymentApiService.GetPaymentByIdAsync(paymentId);
            var receipts = await _paymentApiService.GetReceiptsByPaymentAsync(paymentId);

            Receipts.Clear();
            foreach (var item in receipts)
            {
                Receipts.Add(item);
            }
        }
        catch (Exception ex)
        {
            Message = $"No fue posible cargar el detalle del pago. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateStatusAsync()
    {
        if (!CanManagePayments || Payment is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _paymentApiService.UpdatePaymentStatusAsync(Payment.Id, SelectedStatus?.Value ?? Payment.Status);
            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible actualizar el estado del pago.";
                return;
            }

            IsStatusModalVisible = false;
            Message = response.Message;
            await LoadAsync(Payment.Id);
        }
        catch (Exception ex)
        {
            Message = $"No fue posible actualizar el estado del pago. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UploadReceiptAsync()
    {
        if (!CanUploadReceipts || Payment is null)
        {
            return;
        }

        var fileResult = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona el comprobante"
        });

        if (fileResult is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var response = await _paymentApiService.UploadReceiptAsync(Payment.Id, fileResult);
            if (response?.Success != true || response.Data is null)
            {
                Message = response?.Message ?? "No fue posible subir el comprobante.";
                return;
            }

            IsReceiptModalVisible = false;
            Message = response.Message;
            await LoadAsync(Payment.Id);
        }
        catch (Exception ex)
        {
            Message = $"No fue posible subir el comprobante. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DownloadReceiptAsync(ReceiptModel? receipt)
    {
        if (receipt is null)
        {
            return;
        }

        try
        {
            var filePath = await _paymentApiService.DownloadReceiptAsync(receipt.Id, receipt.FileName);
            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
        }
        catch (Exception ex)
        {
            Message = $"No fue posible descargar el comprobante. {ex.Message}";
        }
    }

    private Task OpenStatusModalAsync()
    {
        if (Payment is not null)
        {
            SelectedStatus = PaymentStatuses.FirstOrDefault(x => x.Value == Payment.Status)
                ?? PaymentStatuses.FirstOrDefault();
        }

        IsStatusModalVisible = true;
        return Task.CompletedTask;
    }

    private Task CloseStatusModalAsync()
    {
        IsStatusModalVisible = false;
        return Task.CompletedTask;
    }

    private Task OpenReceiptModalAsync()
    {
        IsReceiptModalVisible = true;
        return Task.CompletedTask;
    }

    private Task CloseReceiptModalAsync()
    {
        IsReceiptModalVisible = false;
        return Task.CompletedTask;
    }
}
