using System.Text.Json;
using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services;

public class SessionStorageService
{
    private const string SessionKey = "bakerypay_session";

    public async Task SaveSessionAsync(AuthSession session)
    {
        var json = JsonSerializer.Serialize(session);
        await SecureStorage.Default.SetAsync(SessionKey, json);
    }

    public async Task<AuthSession?> GetSessionAsync()
    {
        try
        {
            var json = await SecureStorage.Default.GetAsync(SessionKey);
            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<AuthSession>(json);
        }
        catch
        {
            return null;
        }
    }

    public void ClearSession() => SecureStorage.Default.Remove(SessionKey);
}
