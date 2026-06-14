using System.Collections.ObjectModel;
using BakeryPay.Mobile.Models;
using BakeryPay.Mobile.Services;

namespace BakeryPay.Mobile.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly DashboardApiService _dashboardApiService;
    private readonly SessionStorageService _sessionStorageService;
    private decimal _totalAmountPaid;
    private int _totalPayments;
    private int _totalProviders;
    private int _pendingNotifications;
    private bool _canManageProviders;
    private bool _canManageUsers;

    public DashboardViewModel(DashboardApiService dashboardApiService, SessionStorageService sessionStorageService)
    {
        _dashboardApiService = dashboardApiService;
        _sessionStorageService = sessionStorageService;
        Title = "Resumen";
        RecentPayments = new ObservableCollection<RecentPaymentModel>();
        RefreshCommand = new AsyncCommand(LoadAsync);
        OpenProvidersCommand = new AsyncCommand(OpenProvidersAsync);
        OpenUsersCommand = new AsyncCommand(OpenUsersAsync);
        OpenRolesCommand = new AsyncCommand(OpenRolesAsync);
    }

    public int TotalProviders
    {
        get => _totalProviders;
        set => SetProperty(ref _totalProviders, value);
    }

    public int TotalPayments
    {
        get => _totalPayments;
        set => SetProperty(ref _totalPayments, value);
    }

    public decimal TotalAmountPaid
    {
        get => _totalAmountPaid;
        set => SetProperty(ref _totalAmountPaid, value);
    }

    public int PendingNotifications
    {
        get => _pendingNotifications;
        set => SetProperty(ref _pendingNotifications, value);
    }

    public bool CanManageProviders
    {
        get => _canManageProviders;
        set => SetProperty(ref _canManageProviders, value);
    }

    public bool CanManageUsers
    {
        get => _canManageUsers;
        set => SetProperty(ref _canManageUsers, value);
    }

    public ObservableCollection<RecentPaymentModel> RecentPayments { get; }
    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand OpenProvidersCommand { get; }
    public AsyncCommand OpenUsersCommand { get; }
    public AsyncCommand OpenRolesCommand { get; }

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            var session = await _sessionStorageService.GetSessionAsync();
            CanManageProviders = session?.Role is "Administrator" or "Cashier";
            CanManageUsers = session?.Role is "Administrator";

            var summary = await _dashboardApiService.GetSummaryAsync();
            if (summary is null)
            {
                return;
            }

            TotalProviders = summary.TotalProviders;
            TotalPayments = summary.TotalPayments;
            TotalAmountPaid = summary.TotalAmountPaid;
            PendingNotifications = summary.PendingNotifications;

            RecentPayments.Clear();
            foreach (var item in summary.RecentPayments)
            {
                RecentPayments.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task OpenProvidersAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.ProvidersPage));
    }

    private static async Task OpenUsersAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.UsersPage));
    }

    private static async Task OpenRolesAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.RolesPage));
    }
}
