using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<LoginViewModel>();
        BindingContext = _viewModel;
    }
}
