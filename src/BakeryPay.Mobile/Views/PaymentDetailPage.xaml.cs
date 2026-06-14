using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

[QueryProperty(nameof(PaymentId), "paymentId")]
public partial class PaymentDetailPage : ContentPage
{
    private readonly PaymentDetailViewModel _viewModel;
    private string _paymentId = string.Empty;

    public PaymentDetailPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<PaymentDetailViewModel>();
        BindingContext = _viewModel;
    }

    public string PaymentId
    {
        get => _paymentId;
        set
        {
            _paymentId = value;
            if (Guid.TryParse(value, out var parsedId))
            {
                MainThread.BeginInvokeOnMainThread(async () => await _viewModel.LoadAsync(parsedId));
            }
        }
    }
}
