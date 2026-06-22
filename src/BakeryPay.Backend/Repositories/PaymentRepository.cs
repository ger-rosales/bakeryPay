using BakeryPay.Backend.DTOs.Dashboard;
using BakeryPay.Backend.Interfaces.Repositories;
using BakeryPay.Backend.Entities;
using BakeryPay.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryPay.Backend.Repositories;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(BakeryPayDbContext context)
        : base(context)
    {
    }

    public override async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Payments
            .Include(x => x.Provider)
            .Include(x => x.RegisteredByUser)
            .Include(x => x.Receipts)
            .OrderByDescending(x => x.PaymentDateUtc)
            .ToListAsync(cancellationToken);

    public override async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Payments
            .Include(x => x.Provider)
            .Include(x => x.RegisteredByUser)
            .Include(x => x.Receipts)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default) =>
        await Context.Payments
            .Include(x => x.Provider)
            .Include(x => x.RegisteredByUser)
            .Include(x => x.Receipts)
            .Where(x => x.ProviderId == providerId)
            .OrderByDescending(x => x.PaymentDateUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RecentPaymentDto>> GetRecentPaymentsAsync(int take, CancellationToken cancellationToken = default) =>
        await Context.Payments
            .Include(x => x.Provider)
            .OrderByDescending(x => x.PaymentDateUtc)
            .Take(take)
            .Select(x => new RecentPaymentDto
            {
                PaymentId = x.Id,
                ProviderName = x.Provider!.BusinessName,
                Amount = x.Amount,
                PaymentDateUtc = x.PaymentDateUtc,
                ReferenceNumber = x.ReferenceNumber
            })
            .ToListAsync(cancellationToken);

    public async Task<bool> ReferenceExistsAsync(string referenceNumber, CancellationToken cancellationToken = default) =>
        await Context.Payments.AnyAsync(x => x.ReferenceNumber == referenceNumber, cancellationToken);
}
