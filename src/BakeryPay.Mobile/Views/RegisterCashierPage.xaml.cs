using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

public partial class RegisterCashierPage : ContentPage
{
    private readonly RegisterCashierViewModel _viewModel;

    public RegisterCashierPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<RegisterCashierViewModel>();
        BindingContext = _viewModel;
    }
}
