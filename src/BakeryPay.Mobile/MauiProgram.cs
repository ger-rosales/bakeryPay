using BakeryPay.Mobile.Services;
using BakeryPay.Mobile.Services.Biometric;
using BakeryPay.Mobile.ViewModels;
using BakeryPay.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace BakeryPay.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton(new ApiOptions
        {
            BaseUrl = "http://192.168.1.18:5162"
        });

        builder.Services.AddSingleton<SessionStorageService>();
        builder.Services.AddSingleton<DeviceInstallationService>();
        builder.Services.AddSingleton<IBiometricService, NativeBiometricService>();

        builder.Services.AddSingleton(sp =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(sp.GetRequiredService<ApiOptions>().BaseUrl)
            };

            return httpClient;
        });

        builder.Services.AddSingleton<AuthApiService>();
        builder.Services.AddSingleton<DashboardApiService>();
        builder.Services.AddSingleton<PaymentApiService>();
        builder.Services.AddSingleton<NotificationApiService>();
        builder.Services.AddSingleton<ProviderApiService>();
        builder.Services.AddSingleton<UserApiService>();
        builder.Services.AddSingleton<RoleApiService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterCashierViewModel>();
        builder.Services.AddTransient<ChangePasswordViewModel>();
        builder.Services.AddTransient<SecurityViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<PaymentsViewModel>();
        builder.Services.AddTransient<PaymentDetailViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<ProvidersViewModel>();
        builder.Services.AddTransient<UsersViewModel>();
        builder.Services.AddTransient<RolesViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterCashierPage>();
        builder.Services.AddTransient<ChangePasswordPage>();
        builder.Services.AddTransient<SecurityPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<PaymentsPage>();
        builder.Services.AddTransient<PaymentDetailPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<ProvidersPage>();
        builder.Services.AddTransient<UsersPage>();
        builder.Services.AddTransient<RolesPage>();

        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        ServiceHelper.Initialize(app.Services);
        return app;
    }
}
