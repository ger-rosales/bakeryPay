using BakeryPay.Backend.Entities;

namespace BakeryPay.Backend.Interfaces.Repositories;

public interface IUserBiometricRepository : IGenericRepository<UserBiometricCredential>
{
    Task<UserBiometricCredential?> GetActiveByUserAndDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserBiometricCredential>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
