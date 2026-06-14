using BakeryPay.Application.Interfaces.Repositories;
using BakeryPay.Domain.Entities;
using BakeryPay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BakeryPay.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(BakeryPayDbContext context)
        : base(context)
    {
    }

    public override async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Roles
            .Include(x => x.Users)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public override async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Roles
            .Include(x => x.Users)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await Context.Roles.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
}
