using BakeryPay.Domain.Entities;

namespace BakeryPay.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid excludingUserId, CancellationToken cancellationToken = default);
    Task<bool> IdentificationExistsAsync(string identificationNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsByRoleIdAsync(Guid roleId, Guid? excludingUserId = null, CancellationToken cancellationToken = default);
}
