using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services;

public class RoleApiService : BaseApiService
{
    public RoleApiService(HttpClient httpClient, SessionStorageService sessionStorageService)
        : base(httpClient, sessionStorageService)
    {
    }

    public async Task<List<RoleModel>> GetRolesAsync() =>
        await GetAsync<List<RoleModel>>("api/roles") ?? new List<RoleModel>();

    public Task<ApiEnvelope<RoleModel>?> CreateRoleAsync(string name, string description) =>
        PostAsync<object, ApiEnvelope<RoleModel>>("api/roles", new
        {
            name,
            description
        });

    public Task<ApiEnvelope<RoleModel>?> UpdateRoleAsync(Guid id, string name, string description) =>
        PutAsync<object, ApiEnvelope<RoleModel>>($"api/roles/{id}", new
        {
            name,
            description
        });

    public Task<ApiEnvelope<bool>?> DeleteRoleAsync(Guid id) =>
        DeleteAsync<ApiEnvelope<bool>>($"api/roles/{id}");
}
