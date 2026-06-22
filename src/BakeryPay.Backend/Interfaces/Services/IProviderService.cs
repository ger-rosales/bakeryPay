using BakeryPay.Backend.Common;
using BakeryPay.Backend.DTOs.Providers;

namespace BakeryPay.Backend.Interfaces.Services;

public interface IProviderService
{
    Task<IReadOnlyList<ProviderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProviderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<CreateProviderResultDto>> CreateAsync(CreateProviderDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProviderDto>> UpdateAsync(Guid id, UpdateProviderDto dto, CancellationToken cancellationToken = default);
}
