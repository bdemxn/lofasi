using Lofasi.Application.Abstractions.Authentication;
using Lofasi.Application.Abstractions.Clock;
using Lofasi.Application.Abstractions.Persistence;
using Lofasi.Application.Abstractions.Services;
using Lofasi.Application.Accounts.Dtos;
using Lofasi.Application.Exceptions;
using Lofasi.Application.Transactions.Dtos;
using Lofasi.Domain.Entities;
using Lofasi.Domain.ValueObjects;

namespace Lofasi.Application.Accounts;

public sealed class AccountService(
    IAccountRepository accountRepository,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IAccountNumberGenerator accountNumberGenerator,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IAccountService
{
    private const int AccountNumberGenerationAttempts = 10;

    public async Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var customer = await customerRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Customer profile was not found.");

        var createdAtUtc = dateTimeProvider.UtcNow;
        var accountNumber = await GenerateUniqueAccountNumberAsync(createdAtUtc, cancellationToken);
        var openingBalanceInCents = ToCents(request.OpeningBalance);

        var account = new BankAccount(
            customer.Id,
            accountNumber,
            openingBalanceInCents,
            createdAtUtc);

        await accountRepository.AddAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapAccount(account);
    }

    public async Task<AccountBalanceResponse> GetBalanceAsync(string accountNumber, CancellationToken cancellationToken)
    {
        var account = await GetOwnedAccountAsync(accountNumber, cancellationToken);

        return new AccountBalanceResponse(
            account.AccountNumber,
            Money.FromCents(account.BalanceInCents),
            account.BalanceInCents);
    }

    public async Task<TransactionResponse> DepositAsync(
        string accountNumber,
        TransactionRequest request,
        CancellationToken cancellationToken)
    {
        var account = await GetOwnedAccountAsync(accountNumber, cancellationToken);
        var amountInCents = ToPositiveCents(request.Amount);
        var transaction = account.Deposit(amountInCents, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapTransaction(transaction);
    }

    public async Task<TransactionResponse> WithdrawAsync(
        string accountNumber,
        TransactionRequest request,
        CancellationToken cancellationToken)
    {
        var account = await GetOwnedAccountAsync(accountNumber, cancellationToken);
        var amountInCents = ToPositiveCents(request.Amount);

        if (account.BalanceInCents < amountInCents)
        {
            throw new InsufficientFundsException("Insufficient funds.");
        }

        var transaction = account.Withdraw(amountInCents, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapTransaction(transaction);
    }

    public async Task<IReadOnlyCollection<TransactionResponse>> GetTransactionHistoryAsync(
        string accountNumber,
        CancellationToken cancellationToken)
    {
        var account = await GetOwnedAccountAsync(accountNumber, cancellationToken);

        return [.. account.Transactions
            .OrderBy(transaction => transaction.OccurredAtUtc)
            .Select(MapTransaction)];
    }

    private async Task<BankAccount> GetOwnedAccountAsync(string accountNumber, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var account = await accountRepository.GetByAccountNumberForUserAsync(
            accountNumber,
            userId,
            cancellationToken)
            ?? throw new NotFoundException("Account was not found.");

        return account;
    }

    private Guid GetCurrentUserId()
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthenticatedException("Authentication is required.");
        }

        return currentUserService.UserId;
    }

    private async Task<string> GenerateUniqueAccountNumberAsync(
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken)
    {
        var creationDate = DateOnly.FromDateTime(createdAtUtc.UtcDateTime);

        for (var attempt = 0; attempt < AccountNumberGenerationAttempts; attempt++)
        {
            var accountNumber = accountNumberGenerator.Generate(creationDate);

            if (!await accountRepository.ExistsByAccountNumberAsync(accountNumber, cancellationToken))
            {
                return accountNumber;
            }
        }

        throw new BusinessException("Unable to generate a unique account number.");
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

    private static long ToPositiveCents(decimal amount)
    {
        var cents = ToCents(amount);

        if (cents <= 0)
        {
            throw new ValidationException("Amount must be greater than zero.");
        }

        return cents;
    }

    private static AccountResponse MapAccount(BankAccount account)
    {
        return new AccountResponse(
            account.Id,
            account.AccountNumber,
            Money.FromCents(account.BalanceInCents),
            account.BalanceInCents,
            account.CreatedAtUtc);
    }

    private static TransactionResponse MapTransaction(AccountTransaction transaction)
    {
        return new TransactionResponse(
            transaction.Id,
            transaction.Type,
            Money.FromCents(transaction.AmountInCents),
            transaction.AmountInCents,
            transaction.OccurredAtUtc,
            Money.FromCents(transaction.BalanceAfterTransactionInCents),
            transaction.BalanceAfterTransactionInCents);
    }
}
