using Lofasi.Application.Abstractions.Authentication;
using Lofasi.Application.Abstractions.Clock;
using Lofasi.Application.Abstractions.Persistence;
using Lofasi.Application.Abstractions.Services;
using Lofasi.Application.Accounts;
using Lofasi.Application.Exceptions;
using Lofasi.Application.Transactions.Dtos;
using Lofasi.Domain.Entities;
using Lofasi.Domain.Enums;
using Xunit;

namespace Lofasi.UnitTests;

public sealed class AccountServiceTests
{
    private static readonly Guid UserId = Guid.Parse("1f71e466-a7d9-4a92-868e-c37e28b9b4f7");
    private static readonly DateTimeOffset Now = new(2026, 7, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task DepositAsync_ShouldIncreaseBalanceAndRecordTransaction()
    {
        var account = CreateAccount(openingBalanceInCents: 1_000);
        var service = CreateService(account);

        var transaction = await service.DepositAsync(
            account.AccountNumber,
            new TransactionRequest(10.99m),
            CancellationToken.None);

        Assert.Equal(2_099, account.BalanceInCents);
        Assert.Equal(TransactionType.Deposit, transaction.Type);
        Assert.Equal(10.99m, transaction.Amount);
        Assert.Equal(1_099, transaction.AmountInCents);
        Assert.Equal(20.99m, transaction.BalanceAfterTransaction);
        Assert.Equal(2_099, transaction.BalanceAfterTransactionInCents);
        Assert.Equal(2, account.Transactions.Count);
    }

    [Fact]
    public async Task WithdrawAsync_WithSufficientFunds_ShouldDecreaseBalanceAndRecordTransaction()
    {
        var account = CreateAccount(openingBalanceInCents: 5_000);
        var service = CreateService(account);

        var transaction = await service.WithdrawAsync(
            account.AccountNumber,
            new TransactionRequest(12.50m),
            CancellationToken.None);

        Assert.Equal(3_750, account.BalanceInCents);
        Assert.Equal(TransactionType.Withdrawal, transaction.Type);
        Assert.Equal(12.50m, transaction.Amount);
        Assert.Equal(1_250, transaction.AmountInCents);
        Assert.Equal(37.50m, transaction.BalanceAfterTransaction);
        Assert.Equal(3_750, transaction.BalanceAfterTransactionInCents);
        Assert.Equal(2, account.Transactions.Count);
    }

    [Fact]
    public async Task WithdrawAsync_WithInsufficientFunds_ShouldRejectTransactionCleanly()
    {
        var account = CreateAccount(openingBalanceInCents: 500);
        var service = CreateService(account);

        var exception = await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            service.WithdrawAsync(
                account.AccountNumber,
                new TransactionRequest(10.00m),
                CancellationToken.None));

        Assert.Equal("Insufficient funds.", exception.Message);
        Assert.Equal(500, account.BalanceInCents);
        Assert.Single(account.Transactions);
    }

    private static AccountService CreateService(BankAccount account)
    {
        return new AccountService(
            new InMemoryAccountRepository(account),
            new InMemoryCustomerRepository(),
            new TestUnitOfWork(),
            new TestAccountNumberGenerator(),
            new TestCurrentUserService(),
            new TestDateTimeProvider());
    }

    private static BankAccount CreateAccount(long openingBalanceInCents)
    {
        var customer = new Customer(
            UserId,
            "Jane Doe",
            new DateOnly(1990, 5, 10),
            Gender.Female,
            monthlyIncomeInCents: 450_000,
            Now);

        return new BankAccount(
            customer.Id,
            "ACC-20260715-0001",
            openingBalanceInCents,
            Now);
    }

    private sealed class InMemoryAccountRepository(BankAccount account) : IAccountRepository
    {
        public Task AddAsync(BankAccount account, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken)
        {
            return Task.FromResult(account.AccountNumber == accountNumber);
        }

        public Task<BankAccount?> GetByAccountNumberForUserAsync(
            string accountNumber,
            Guid userId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(account.AccountNumber == accountNumber && userId == UserId ? account : null);
        }
    }

    private sealed class InMemoryCustomerRepository : ICustomerRepository
    {
        public Task AddAsync(Customer customer, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<Customer?>(null);
        }
    }

    private sealed class TestUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(1);
        }
    }

    private sealed class TestAccountNumberGenerator : IAccountNumberGenerator
    {
        public string Generate(DateOnly creationDate)
        {
            return $"ACC-{creationDate:yyyyMMdd}-9999";
        }
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public Guid UserId => AccountServiceTests.UserId;

        public bool IsAuthenticated => true;
    }

    private sealed class TestDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => Now;
    }
}
