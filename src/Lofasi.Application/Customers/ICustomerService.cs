using Lofasi.Application.Customers.Dtos;

namespace Lofasi.Application.Customers;

public interface ICustomerService
{
    Task<CustomerResponse> GetCurrentAsync(CancellationToken cancellationToken);
}
