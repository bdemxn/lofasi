using Lofasi.Application.Abstractions.Authentication;
using Lofasi.Application.Abstractions.Clock;
using Lofasi.Application.Auth;
using Lofasi.Application.Auth.Dtos;
using Lofasi.Application.Exceptions;
using Lofasi.Domain.Entities;
using Lofasi.Domain.ValueObjects;
using Lofasi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Lofasi.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider,
    BankingDbContext dbContext) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterCustomerRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();

        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            throw new ConflictException("A user with the supplied email already exists.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true
        };

        var monthlyIncomeInCents = ToCents(request.MonthlyIncome);
        var customer = new Customer(
            user.Id,
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            monthlyIncomeInCents,
            dateTimeProvider.UtcNow);

        user.CustomerId = customer.Id;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new ValidationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        await dbContext.Customers.AddAsync(customer, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreateAuthResponse(user, customer.Id);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim())
            ?? throw new InvalidCredentialsException("Invalid email or password.");

        var passwordIsValid = await userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordIsValid)
        {
            throw new InvalidCredentialsException("Invalid email or password.");
        }

        if (user.CustomerId is null)
        {
            throw new NotFoundException("Customer profile was not found.");
        }

        return CreateAuthResponse(user, user.CustomerId.Value);
    }

    private AuthResponse CreateAuthResponse(ApplicationUser user, Guid customerId)
    {
        var token = jwtTokenService.CreateToken(user.Id, user.Email ?? string.Empty);

        return new AuthResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            user.Id,
            customerId,
            user.Email ?? string.Empty);
    }

    private static long ToCents(decimal amount)
    {
        try
        {
            return Money.ToCents(amount);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }
    }
}
