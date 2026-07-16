using Lofasi.Application.Abstractions.Persistence;
using Lofasi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lofasi.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(BankingDbContext dbContext) : ICustomerRepository
{
    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        await dbContext.Customers.AddAsync(customer, cancellationToken);
    }

    public async Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Customers
            .SingleOrDefaultAsync(customer => customer.UserId == userId, cancellationToken);
    }
}
