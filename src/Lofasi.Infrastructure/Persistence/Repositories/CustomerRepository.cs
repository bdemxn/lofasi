using Lofasi.Application.Abstractions.Persistence;
using Lofasi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lofasi.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly BankingDbContext _dbContext;

    public CustomerRepository(BankingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        await _dbContext.Customers.AddAsync(customer, cancellationToken);
    }

    public async Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .SingleOrDefaultAsync(customer => customer.UserId == userId, cancellationToken);
    }
}
