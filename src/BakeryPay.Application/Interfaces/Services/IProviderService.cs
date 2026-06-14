using BakeryPay.Application.Common;
using BakeryPay.Application.DTOs.Providers;

namespace BakeryPay.Application.Interfaces.Services;

public interface IProviderService
{
    Task<IReadOnlyList<ProviderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProviderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<CreateProviderResultDto>> CreateAsync(CreateProviderDto dto, CancellationToken cancellationToken = default);
}
