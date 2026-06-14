using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

public partial class RolesPage : ContentPage
{
    private readonly RolesViewModel _viewModel;

    public RolesPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<RolesViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
