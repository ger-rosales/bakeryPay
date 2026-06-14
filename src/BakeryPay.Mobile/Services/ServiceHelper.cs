namespace BakeryPay.Mobile.Services;

public static class ServiceHelper
{
    private static IServiceProvider? _serviceProvider;

    public static void Initialize(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public static T GetService<T>() where T : notnull
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("Service provider not initialized.");
        }

        return _serviceProvider.GetRequiredService<T>();
    }
}
