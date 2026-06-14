using BakeryPay.Domain.Entities;

namespace BakeryPay.Application.Interfaces.Repositories;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
