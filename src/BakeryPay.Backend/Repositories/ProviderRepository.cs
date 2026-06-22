using BakeryPay.Backend.Interfaces.Repositories;
using BakeryPay.Backend.Entities;
using BakeryPay.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryPay.Backend.Repositories;

public class ProviderRepository : GenericRepository<Provider>, IProviderRepository
{
    public ProviderRepository(BakeryPayDbContext context)
        : base(context)
    {
    }

    public override async Task<IReadOnlyList<Provider>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Providers
            .Include(x => x.Users)
            .Include(x => x.Notifications)
            .OrderBy(x => x.BusinessName)
            .ToListAsync(cancellationToken);

    public override async Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Providers
            .Include(x => x.Users)
                .ThenInclude(x => x.Role)
            .Include(x => x.Notifications)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> TaxIdExistsAsync(string taxId, CancellationToken cancellationToken = default) =>
        await Context.Providers.AnyAsync(x => x.TaxId == taxId, cancellationToken);

    public async Task<bool> TaxIdExistsAsync(string taxId, Guid excludingProviderId, CancellationToken cancellationToken = default) =>
        await Context.Providers.AnyAsync(x => x.TaxId == taxId && x.Id != excludingProviderId, cancellationToken);

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        await Context.Providers.AnyAsync(x => x.Code == code, cancellationToken);

    public async Task<Provider?> GetByCodeAndTaxIdAsync(string code, string taxId, CancellationToken cancellationToken = default) =>
        await Context.Providers
            .Include(x => x.Users)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(
                x => x.Code == code && x.TaxId == taxId && x.IsActive,
                cancellationToken);
}
