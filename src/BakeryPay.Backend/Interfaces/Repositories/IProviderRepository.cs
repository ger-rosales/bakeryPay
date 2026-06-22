using BakeryPay.Backend.Entities;

namespace BakeryPay.Backend.Interfaces.Repositories;

public interface IProviderRepository : IGenericRepository<Provider>
{
    Task<bool> TaxIdExistsAsync(string taxId, CancellationToken cancellationToken = default);
    Task<bool> TaxIdExistsAsync(string taxId, Guid excludingProviderId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<Provider?> GetByCodeAndTaxIdAsync(string code, string taxId, CancellationToken cancellationToken = default);
}
