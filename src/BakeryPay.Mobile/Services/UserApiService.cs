using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services;

public class UserApiService : BaseApiService
{
    public UserApiService(HttpClient httpClient, SessionStorageService sessionStorageService)
        : base(httpClient, sessionStorageService)
    {
    }

    public async Task<List<UserModel>> GetUsersAsync() =>
        await GetAsync<List<UserModel>>("api/users") ?? new List<UserModel>();

    public Task<ApiEnvelope<UserModel>?> CreateUserAsync(
        string firstName,
        string lastName,
        string identificationNumber,
        string email,
        string password,
        string phone,
        Guid roleId) =>
        PostAsync<object, ApiEnvelope<UserModel>>("api/users", new
        {
            firstName,
            lastName,
            identificationNumber,
            email,
            password,
            phone,
            roleId
        });

    public Task<ApiEnvelope<UserModel>?> UpdateUserAsync(
        Guid id,
        string firstName,
        string lastName,
        string identificationNumber,
        string email,
        string phone,
        Guid roleId) =>
        PutAsync<object, ApiEnvelope<UserModel>>($"api/users/{id}", new
        {
            firstName,
            lastName,
            identificationNumber,
            email,
            phone,
            roleId
        });

    public Task<ApiEnvelope<UserModel>?> SetUserStatusAsync(Guid id, bool isActive) =>
        PostAsync<object, ApiEnvelope<UserModel>>($"api/users/{id}/status", new
        {
            isActive
        });

    public Task<ApiEnvelope<ResetUserPasswordResultModel>?> ResetPasswordAsync(Guid id) =>
        PostAsync<object, ApiEnvelope<ResetUserPasswordResultModel>>($"api/users/{id}/reset-password", new { });
}
