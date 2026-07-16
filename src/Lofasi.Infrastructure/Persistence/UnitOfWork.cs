using Lofasi.Application.Abstractions.Persistence;

namespace Lofasi.Infrastructure.Persistence;

public sealed class UnitOfWork(BankingDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
