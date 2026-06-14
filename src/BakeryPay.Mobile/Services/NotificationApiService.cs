using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services;

public class NotificationApiService : BaseApiService
{
    public NotificationApiService(HttpClient httpClient, SessionStorageService sessionStorageService)
        : base(httpClient, sessionStorageService)
    {
    }

    public async Task<List<NotificationModel>> GetByProviderAsync(Guid providerId) =>
        await GetAsync<List<NotificationModel>>($"api/notifications/provider/{providerId}") ?? new List<NotificationModel>();

    public Task<ApiEnvelope<NotificationModel>?> CreateAsync(Guid providerId, string title, string message, string type) =>
        PostAsync<object, ApiEnvelope<NotificationModel>>("api/notifications", new
        {
            providerId,
            title,
            message,
            type
        });

    public Task MarkAsReadAsync(Guid notificationId) =>
        PutAsync($"api/notifications/{notificationId}/read");
}
