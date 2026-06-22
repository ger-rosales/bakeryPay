using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services;

public class ProviderApiService : BaseApiService
{
    public ProviderApiService(HttpClient httpClient, SessionStorageService sessionStorageService)
        : base(httpClient, sessionStorageService)
    {
    }

    public async Task<List<ProviderModel>> GetProvidersAsync() =>
        await GetAsync<List<ProviderModel>>("api/providers") ?? new List<ProviderModel>();

    public Task<ApiEnvelope<CreateProviderResultModel>?> CreateProviderAsync(
        string code,
        string businessName,
        string taxId,
        string contactFirstName,
        string contactLastName,
        string contactIdentificationNumber,
        string contactEmail,
        string contactPhone) =>
        PostAsync<object, ApiEnvelope<CreateProviderResultModel>>("api/providers", new
        {
            code,
            businessName,
            taxId,
            contactFirstName,
            contactLastName,
            contactIdentificationNumber,
            contactEmail,
            contactPhone
        });

    public Task<ApiEnvelope<ProviderModel>?> UpdateProviderAsync(
        Guid providerId,
        string taxId,
        string contactFirstName,
        string contactLastName,
        string contactEmail,
        string contactPhone) =>
        PutAsync<object, ApiEnvelope<ProviderModel>>($"api/providers/{providerId}", new
        {
            taxId,
            contactFirstName,
            contactLastName,
            contactEmail,
            contactPhone
        });
}
