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

public sealed class AccountService : IAccountService
{
    private const int AccountNumberGenerationAttempts = 10;

    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountNumberGenerator _accountNumberGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AccountService(
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IAccountNumberGenerator accountNumberGenerator,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _accountNumberGenerator = accountNumberGenerator;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var customer = await _customerRepository.GetByUserIdAsync(userId, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException("Customer profile was not found.");
        }

        var createdAtUtc = _dateTimeProvider.UtcNow;
        var accountNumber = await GenerateUniqueAccountNumberAsync(createdAtUtc, cancellationToken);
        var openingBalanceInCents = ToCents(request.OpeningBalance);

        var account = new BankAccount(
            customer.Id,
            accountNumber,
            openingBalanceInCents,
            createdAtUtc);

        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var transaction = account.Deposit(amountInCents, _dateTimeProvider.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        var transaction = account.Withdraw(amountInCents, _dateTimeProvider.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapTransaction(transaction);
    }

    public async Task<IReadOnlyCollection<TransactionResponse>> GetTransactionHistoryAsync(
        string accountNumber,
        CancellationToken cancellationToken)
    {
        var account = await GetOwnedAccountAsync(accountNumber, cancellationToken);

        return account.Transactions
            .OrderBy(transaction => transaction.OccurredAtUtc)
            .Select(MapTransaction)
            .ToArray();
    }

    private async Task<BankAccount> GetOwnedAccountAsync(string accountNumber, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var account = await _accountRepository.GetByAccountNumberForUserAsync(
            accountNumber,
            userId,
            cancellationToken);

        if (account is null)
        {
            throw new NotFoundException("Account was not found.");
        }

        return account;
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthenticatedException("Authentication is required.");
        }

        return _currentUserService.UserId;
    }

    private async Task<string> GenerateUniqueAccountNumberAsync(
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken)
    {
        var creationDate = DateOnly.FromDateTime(createdAtUtc.UtcDateTime);

        for (var attempt = 0; attempt < AccountNumberGenerationAttempts; attempt++)
        {
            var accountNumber = _accountNumberGenerator.Generate(creationDate);

            if (!await _accountRepository.ExistsByAccountNumberAsync(accountNumber, cancellationToken))
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
