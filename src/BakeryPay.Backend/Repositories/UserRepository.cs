using BakeryPay.Backend.Interfaces.Repositories;
using BakeryPay.Backend.Entities;
using BakeryPay.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryPay.Backend.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(BakeryPayDbContext context)
        : base(context)
    {
    }

    public override async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Users
            .Include(x => x.Role)
            .Include(x => x.Provider)
            .Include(x => x.BiometricCredentials)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync(cancellationToken);

    public override async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Users
            .Include(x => x.Role)
            .Include(x => x.Provider)
            .Include(x => x.BiometricCredentials)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await Context.Users
            .Include(x => x.Role)
            .Include(x => x.Provider)
            .Include(x => x.BiometricCredentials)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        await Context.Users.AnyAsync(x => x.Email == email, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, Guid excludingUserId, CancellationToken cancellationToken = default) =>
        await Context.Users.AnyAsync(x => x.Email == email && x.Id != excludingUserId, cancellationToken);

    public async Task<bool> IdentificationExistsAsync(string identificationNumber, CancellationToken cancellationToken = default) =>
        await Context.Users.AnyAsync(x => x.IdentificationNumber == identificationNumber, cancellationToken);

    public async Task<bool> ExistsByRoleIdAsync(Guid roleId, Guid? excludingUserId = null, CancellationToken cancellationToken = default) =>
        await Context.Users.AnyAsync(
            x => x.RoleId == roleId && (!excludingUserId.HasValue || x.Id != excludingUserId.Value),
            cancellationToken);
}
