using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services;

public abstract class BaseApiService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly SessionStorageService _sessionStorageService;

    protected BaseApiService(HttpClient httpClient, SessionStorageService sessionStorageService)
    {
        _httpClient = httpClient;
        _sessionStorageService = sessionStorageService;
    }

    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        await ApplyTokenAsync();
        using var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, SerializerOptions);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload)
    {
        await ApplyTokenAsync();
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(endpoint, content);
        var responseJson = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            response.EnsureSuccessStatusCode();
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(responseJson, SerializerOptions);
    }

    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest payload)
    {
        await ApplyTokenAsync();
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PutAsync(endpoint, content);
        var responseJson = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            response.EnsureSuccessStatusCode();
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(responseJson, SerializerOptions);
    }

    protected async Task<TResponse?> DeleteAsync<TResponse>(string endpoint)
    {
        await ApplyTokenAsync();
        using var response = await _httpClient.DeleteAsync(endpoint);
        var responseJson = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            response.EnsureSuccessStatusCode();
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(responseJson, SerializerOptions);
    }

    protected async Task PutAsync(string endpoint)
    {
        await ApplyTokenAsync();
        using var response = await _httpClient.PutAsync(endpoint, null);
        response.EnsureSuccessStatusCode();
    }

    protected async Task<TResponse?> PostMultipartAsync<TResponse>(string endpoint, MultipartFormDataContent content)
    {
        await ApplyTokenAsync();
        using var response = await _httpClient.PostAsync(endpoint, content);
        var responseJson = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            response.EnsureSuccessStatusCode();
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(responseJson, SerializerOptions);
    }

    protected async Task<string> DownloadToCacheAsync(string endpoint, string fileName)
    {
        await ApplyTokenAsync();
        var bytes = await _httpClient.GetByteArrayAsync(endpoint);
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllBytesAsync(filePath, bytes);
        return filePath;
    }

    protected async Task ApplyTokenAsync()
    {
        var session = await _sessionStorageService.GetSessionAsync();
        _httpClient.DefaultRequestHeaders.Authorization = session is null
            ? null
            : new AuthenticationHeaderValue("Bearer", session.Token);
    }
}
