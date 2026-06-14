using BakeryPay.Application.Interfaces.Repositories;
using BakeryPay.Domain.Entities;
using BakeryPay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BakeryPay.Infrastructure.Repositories;

public class ReceiptRepository : GenericRepository<Receipt>, IReceiptRepository
{
    public ReceiptRepository(BakeryPayDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Receipt>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default) =>
        await Context.Receipts
            .Where(x => x.PaymentId == paymentId)
            .OrderByDescending(x => x.UploadedAtUtc)
            .ToListAsync(cancellationToken);
}
