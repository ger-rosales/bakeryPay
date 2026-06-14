namespace BakeryPay.Mobile.Services;

public class DeviceInstallationService
{
    private const string DeviceIdKey = "bakerypay_device_id";

    public async Task<string> GetOrCreateDeviceIdAsync()
    {
        var currentDeviceId = await TryGetSecureValueAsync(DeviceIdKey);
        if (!string.IsNullOrWhiteSpace(currentDeviceId))
        {
            return currentDeviceId;
        }

        currentDeviceId = Preferences.Default.Get(DeviceIdKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(currentDeviceId))
        {
            await TrySetSecureValueAsync(DeviceIdKey, currentDeviceId);
            return currentDeviceId;
        }

        currentDeviceId = Guid.NewGuid().ToString("N");
        Preferences.Default.Set(DeviceIdKey, currentDeviceId);
        await TrySetSecureValueAsync(DeviceIdKey, currentDeviceId);
        return currentDeviceId;
    }

    private static async Task<string?> TryGetSecureValueAsync(string key)
    {
        try
        {
            return await SecureStorage.Default.GetAsync(key);
        }
        catch
        {
            return null;
        }
    }

    private static async Task TrySetSecureValueAsync(string key, string value)
    {
        try
        {
            await SecureStorage.Default.SetAsync(key, value);
        }
        catch
        {
        }
    }
}
