using BakeryPay.Domain.Entities;

namespace BakeryPay.Application.Interfaces.Repositories;

public interface IProviderRepository : IGenericRepository<Provider>
{
    Task<bool> TaxIdExistsAsync(string taxId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<Provider?> GetByCodeAndTaxIdAsync(string code, string taxId, CancellationToken cancellationToken = default);
}
