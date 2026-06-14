using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services;

public class AuthApiService : BaseApiService
{
    public AuthApiService(HttpClient httpClient, SessionStorageService sessionStorageService)
        : base(httpClient, sessionStorageService)
    {
    }

    public Task<ApiEnvelope<AuthSession>?> RegisterCashierAsync(
        string firstName,
        string lastName,
        string identificationNumber,
        string email,
        string phone,
        string password,
        string confirmPassword,
        bool acceptPolicies) =>
        PostAsync<object, ApiEnvelope<AuthSession>>("api/auth/register-cashier", new
        {
            firstName,
            lastName,
            identificationNumber,
            email,
            phone,
            password,
            confirmPassword,
            acceptPolicies
        });

    public Task<ApiEnvelope<AuthSession>?> LoginAsync(string email, string password) =>
        PostAsync<object, ApiEnvelope<AuthSession>>("api/auth/login", new
        {
            email,
            password
        });

    public Task<ApiEnvelope<AuthSession>?> BiometricLoginAsync(string email, string deviceId, int biometricType) =>
        PostAsync<object, ApiEnvelope<AuthSession>>("api/auth/biometric-login", new
        {
            email,
            deviceId,
            biometricType
        });

    public Task<ApiEnvelope<AuthSession>?> ChangePasswordAsync(
        string currentPassword,
        string newPassword,
        string confirmNewPassword) =>
        PostAsync<object, ApiEnvelope<AuthSession>>("api/auth/change-password", new
        {
            currentPassword,
            newPassword,
            confirmNewPassword
        });

    public Task<ApiEnvelope<AuthSession>?> RegisterBiometricAsync(
        string deviceId,
        string deviceName,
        string platform,
        int biometricType) =>
        PostAsync<object, ApiEnvelope<AuthSession>>("api/auth/register-biometric", new
        {
            deviceId,
            deviceName,
            platform,
            biometricType
        });
}
