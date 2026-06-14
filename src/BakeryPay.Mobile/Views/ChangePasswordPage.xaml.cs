using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

public partial class ChangePasswordPage : ContentPage
{
    private readonly ChangePasswordViewModel _viewModel;

    public ChangePasswordPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<ChangePasswordViewModel>();
        BindingContext = _viewModel;
    }
}
