using BakeryPay.Mobile.Models;

namespace BakeryPay.Mobile.Services;

public class DashboardApiService : BaseApiService
{
    public DashboardApiService(HttpClient httpClient, SessionStorageService sessionStorageService)
        : base(httpClient, sessionStorageService)
    {
    }

    public async Task<DashboardSummaryModel?> GetSummaryAsync() =>
        await GetAsync<DashboardSummaryModel>("api/dashboard/summary");
}
