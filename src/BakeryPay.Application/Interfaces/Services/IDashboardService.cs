using BakeryPay.Application.DTOs.Dashboard;

namespace BakeryPay.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
