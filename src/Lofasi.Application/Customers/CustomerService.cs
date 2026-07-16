using Lofasi.Application.Abstractions.Authentication;
using Lofasi.Application.Abstractions.Persistence;
using Lofasi.Application.Customers.Dtos;
using Lofasi.Application.Exceptions;
using Lofasi.Domain.Entities;
using Lofasi.Domain.ValueObjects;

namespace Lofasi.Application.Customers;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserService _currentUserService;

    public CustomerService(ICustomerRepository customerRepository, ICurrentUserService currentUserService)
    {
        _customerRepository = customerRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CustomerResponse> GetCurrentAsync(CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthenticatedException("Authentication is required.");
        }

        var customer = await _customerRepository.GetByUserIdAsync(_currentUserService.UserId, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException("Customer profile was not found.");
        }

        return MapCustomer(customer);
    }

    private static CustomerResponse MapCustomer(Customer customer)
    {
        return new CustomerResponse(
            customer.Id,
            customer.UserId,
            customer.FullName,
            customer.DateOfBirth,
            customer.Gender,
            Money.FromCents(customer.MonthlyIncomeInCents),
            customer.MonthlyIncomeInCents,
            customer.CreatedAtUtc);
    }
}
