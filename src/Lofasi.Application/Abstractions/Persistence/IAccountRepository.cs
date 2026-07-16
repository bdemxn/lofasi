using Lofasi.Domain.Entities;

namespace Lofasi.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task AddAsync(BankAccount account, CancellationToken cancellationToken);

    Task<bool> ExistsByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken);

    Task<BankAccount?> GetByAccountNumberForUserAsync(
        string accountNumber,
        Guid userId,
        CancellationToken cancellationToken);
}
