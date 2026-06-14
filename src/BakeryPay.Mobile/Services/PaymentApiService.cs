using System.Net.Http.Headers;
using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services;

public class PaymentApiService : BaseApiService
{
    public PaymentApiService(HttpClient httpClient, SessionStorageService sessionStorageService)
        : base(httpClient, sessionStorageService)
    {
    }

    public async Task<List<PaymentModel>> GetAllPaymentsAsync() =>
        await GetAsync<List<PaymentModel>>("api/payments") ?? new List<PaymentModel>();

    public async Task<List<PaymentModel>> GetPaymentsByProviderAsync(Guid providerId) =>
        await GetAsync<List<PaymentModel>>($"api/payments/provider/{providerId}") ?? new List<PaymentModel>();

    public async Task<PaymentModel?> GetPaymentByIdAsync(Guid paymentId) =>
        await GetAsync<PaymentModel>($"api/payments/{paymentId}");

    public Task<ApiEnvelope<PaymentModel>?> CreatePaymentAsync(
        Guid providerId,
        decimal amount,
        string currency,
        DateTime paymentDateUtc,
        string referenceNumber,
        string description,
        string status) =>
        PostAsync<object, ApiEnvelope<PaymentModel>>("api/payments", new
        {
            providerId,
            amount,
            currency,
            paymentDateUtc,
            referenceNumber,
            description,
            status
        });

    public Task<ApiEnvelope<PaymentModel>?> UpdatePaymentStatusAsync(Guid paymentId, string status) =>
        PutAsync<object, ApiEnvelope<PaymentModel>>($"api/payments/{paymentId}/status", new
        {
            status
        });

    public async Task<List<ReceiptModel>> GetReceiptsByPaymentAsync(Guid paymentId) =>
        await GetAsync<List<ReceiptModel>>($"api/receipts/payment/{paymentId}") ?? new List<ReceiptModel>();

    public async Task<ApiEnvelope<ReceiptModel>?> UploadReceiptAsync(Guid paymentId, FileResult fileResult)
    {
        await using var stream = await fileResult.OpenReadAsync();
        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(fileResult.ContentType ?? "application/octet-stream");

        using var content = new MultipartFormDataContent
        {
            { new StringContent(paymentId.ToString()), "PaymentId" },
            { streamContent, "File", fileResult.FileName }
        };

        return await PostMultipartAsync<ApiEnvelope<ReceiptModel>>("api/receipts/upload", content);
    }

    public async Task<string> DownloadReceiptAsync(Guid receiptId, string fileName) =>
        await DownloadToCacheAsync($"api/receipts/download/{receiptId}", fileName);
}
