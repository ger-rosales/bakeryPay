using BakeryPay.Backend.DTOs.Dashboard;

namespace BakeryPay.Backend.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
