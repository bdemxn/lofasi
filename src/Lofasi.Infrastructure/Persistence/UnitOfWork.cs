using Lofasi.Application.Abstractions.Persistence;

namespace Lofasi.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BankingDbContext _dbContext;

    public UnitOfWork(BankingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
