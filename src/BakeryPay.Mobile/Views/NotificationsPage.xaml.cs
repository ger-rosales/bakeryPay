using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel _viewModel;

    public NotificationsPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<NotificationsViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
