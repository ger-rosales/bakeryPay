using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<DashboardViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
