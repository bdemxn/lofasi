using Lofasi.Application.Abstractions.Persistence;
using Lofasi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lofasi.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository(BankingDbContext dbContext) : IAccountRepository
{
    public async Task AddAsync(BankAccount account, CancellationToken cancellationToken)
    {
        await dbContext.BankAccounts.AddAsync(account, cancellationToken);
    }

    public async Task<bool> ExistsByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken)
    {
        return await dbContext.BankAccounts.AnyAsync(
            account => account.AccountNumber == accountNumber,
            cancellationToken);
    }

    public async Task<BankAccount?> GetByAccountNumberForUserAsync(
        string accountNumber,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.BankAccounts
            .Include(account => account.Customer)
            .Include(account => account.Transactions)
            .SingleOrDefaultAsync(
                account => account.AccountNumber == accountNumber && account.Customer != null && account.Customer.UserId == userId,
                cancellationToken);
    }
}
