using Lofasi.Domain.Entities;

namespace Lofasi.Application.Abstractions.Persistence;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken);

    Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
