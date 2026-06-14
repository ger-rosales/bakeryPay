using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

public partial class SecurityPage : ContentPage
{
    private readonly SecurityViewModel _viewModel;

    public SecurityPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<SecurityViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
