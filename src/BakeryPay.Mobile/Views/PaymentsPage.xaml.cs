using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

public partial class PaymentsPage : ContentPage
{
    private readonly PaymentsViewModel _viewModel;

    public PaymentsPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<PaymentsViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
