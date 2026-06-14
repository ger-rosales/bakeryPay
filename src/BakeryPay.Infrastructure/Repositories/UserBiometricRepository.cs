using BakeryPay.Application.Interfaces.Repositories;
using BakeryPay.Domain.Entities;
using BakeryPay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BakeryPay.Infrastructure.Repositories;

public class UserBiometricRepository : GenericRepository<UserBiometricCredential>, IUserBiometricRepository
{
    public UserBiometricRepository(BakeryPayDbContext context)
        : base(context)
    {
    }

    public override async Task<IReadOnlyList<UserBiometricCredential>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.UserBiometricCredentials
            .Include(x => x.User)
            .OrderByDescending(x => x.EnrolledAtUtc)
            .ToListAsync(cancellationToken);

    public override async Task<UserBiometricCredential?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.UserBiometricCredentials
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<UserBiometricCredential?> GetActiveByUserAndDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default) =>
        await Context.UserBiometricCredentials
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.DeviceId == deviceId && x.IsActive,
                cancellationToken);

    public async Task<IReadOnlyList<UserBiometricCredential>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await Context.UserBiometricCredentials
            .Where(x => x.UserId == userId && x.IsActive)
            .OrderByDescending(x => x.EnrolledAtUtc)
            .ToListAsync(cancellationToken);
}
