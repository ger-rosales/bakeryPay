using BakeryPay.Application.DTOs.Dashboard;
using BakeryPay.Application.Interfaces.Repositories;
using BakeryPay.Application.Interfaces.Services;

namespace BakeryPay.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IProviderRepository _providerRepository;
    private readonly IPaymentRepository _paymentRepository;

    public DashboardService(IProviderRepository providerRepository, IPaymentRepository paymentRepository)
    {
        _providerRepository = providerRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var providers = await _providerRepository.GetAllAsync(cancellationToken);
        var payments = await _paymentRepository.GetAllAsync(cancellationToken);
        var recentPayments = await _paymentRepository.GetRecentPaymentsAsync(5, cancellationToken);

        return new DashboardSummaryDto
        {
            TotalProviders = providers.Count,
            TotalPayments = payments.Count,
            TotalAmountPaid = payments.Sum(x => x.Amount),
            PendingNotifications = providers.SelectMany(x => x.Notifications).Count(x => !x.IsRead),
            RecentPayments = recentPayments.ToList()
        };
    }
}
