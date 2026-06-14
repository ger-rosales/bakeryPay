using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

public partial class ProvidersPage : ContentPage
{
    private readonly ProvidersViewModel _viewModel;

    public ProvidersPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<ProvidersViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
