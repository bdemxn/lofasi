using Lofasi.Application.Accounts;
using Lofasi.Application.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace Lofasi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
