using BakeryPay.Backend.Interfaces.Repositories;
using BakeryPay.Backend.Entities;
using BakeryPay.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryPay.Backend.Repositories;

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
