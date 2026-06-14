using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.ViewModels;

namespace BakeryPay.Mobile.Views;

public partial class UsersPage : ContentPage
{
    private readonly UsersViewModel _viewModel;

    public UsersPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<UsersViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
