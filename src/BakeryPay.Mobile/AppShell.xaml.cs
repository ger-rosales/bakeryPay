using BakeryPay.Mobile.Models;
using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.Views;

namespace BakeryPay.Mobile;

public partial class AppShell : Shell
{
    private readonly SessionStorageService _sessionStorageService;
    private readonly ToolbarItem _logoutToolbarItem;
    private bool _initialized;

    public AppShell(SessionStorageService sessionStorageService)
    {
        InitializeComponent();
        _sessionStorageService = sessionStorageService;
        _logoutToolbarItem = new ToolbarItem
        {
            Text = "Salir",
            Order = ToolbarItemOrder.Primary,
            Priority = 0
        };
        _logoutToolbarItem.Clicked += OnLogoutClicked;

        Routing.RegisterRoute(nameof(PaymentDetailPage), typeof(PaymentDetailPage));
        Routing.RegisterRoute(nameof(ProvidersPage), typeof(ProvidersPage));
        Routing.RegisterRoute(nameof(UsersPage), typeof(UsersPage));
        Routing.RegisterRoute(nameof(RolesPage), typeof(RolesPage));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized)
        {
            return;
        }

        _initialized = true;
        try
        {
            await RefreshNavigationAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "Aceptar");
        }
    }

    public async Task RefreshNavigationAsync()
    {
        try
        {
            var session = await _sessionStorageService.GetSessionAsync();
            ApplyRoleVisibility(session);

            var requiresPasswordChange = session is not null && session.MustChangePassword;

            var targetRoute = session is null
                ? "//login"
                : requiresPasswordChange
                    ? "//change-password"
                    : session.Role == "Provider"
                        ? "//main/payments-tab"
                        : "//main/dashboard-tab";

            await GoToAsync(targetRoute);
        }
        catch
        {
            _sessionStorageService.ClearSession();
            ApplyRoleVisibility(null);
            await GoToAsync("//login");
        }
    }

    private void ApplyRoleVisibility(AuthSession? session)
    {
        var isProvider = string.Equals(session?.Role, "Provider", StringComparison.OrdinalIgnoreCase);
        var canAccessMainModules = session is not null && !session.MustChangePassword;
        var showDashboard = canAccessMainModules && !isProvider;

        SetLogoutVisibility(session is not null);
        DashboardTab.IsVisible = showDashboard;
        DashboardShellContent.IsVisible = showDashboard;
        PaymentsTab.IsVisible = canAccessMainModules;
        PaymentsShellContent.IsVisible = canAccessMainModules;
        NotificationsTab.IsVisible = canAccessMainModules;
        NotificationsShellContent.IsVisible = canAccessMainModules;
        SecurityTab.IsVisible = canAccessMainModules;
        SecurityShellContent.IsVisible = canAccessMainModules;
    }

    private void SetLogoutVisibility(bool isVisible)
    {
        if (isVisible)
        {
            if (!ToolbarItems.Contains(_logoutToolbarItem))
            {
                ToolbarItems.Add(_logoutToolbarItem);
            }

            return;
        }

        if (ToolbarItems.Contains(_logoutToolbarItem))
        {
            ToolbarItems.Remove(_logoutToolbarItem);
        }
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlertAsync("Cerrar sesion", "Deseas salir de BakeryPay?", "Si", "No");
        if (!confirm)
        {
            return;
        }

        _sessionStorageService.ClearSession();
        await RefreshNavigationAsync();
    }
}
