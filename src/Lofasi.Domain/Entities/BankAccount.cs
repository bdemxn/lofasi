using Lofasi.Domain.Enums;

namespace Lofasi.Domain.Entities;

public sealed class BankAccount
{
    private readonly List<AccountTransaction> _transactions = [];

    private BankAccount()
    {
        AccountNumber = string.Empty;
    }

    public BankAccount(
        Guid customerId,
        string accountNumber,
        long openingBalanceInCents,
        DateTimeOffset createdAtUtc)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        }

        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new ArgumentException("Account number is required.", nameof(accountNumber));
        }

        if (openingBalanceInCents < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(openingBalanceInCents), "Opening balance cannot be negative.");
        }

        Id = Guid.NewGuid();
        CustomerId = customerId;
        AccountNumber = accountNumber.Trim();
        BalanceInCents = openingBalanceInCents;
        CreatedAtUtc = createdAtUtc;

        if (openingBalanceInCents > 0)
        {
            _transactions.Add(AccountTransaction.CreateDeposit(Id, openingBalanceInCents, openingBalanceInCents, createdAtUtc));
        }
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string AccountNumber { get; private set; }

    public long BalanceInCents { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Customer? Customer { get; private set; }

    public IReadOnlyCollection<AccountTransaction> Transactions => _transactions.AsReadOnly();

    public AccountTransaction Deposit(long amountInCents, DateTimeOffset occurredAtUtc)
    {
        EnsurePositiveAmount(amountInCents);

        BalanceInCents += amountInCents;
        var transaction = AccountTransaction.CreateDeposit(Id, amountInCents, BalanceInCents, occurredAtUtc);
        _transactions.Add(transaction);

        return transaction;
    }

    public AccountTransaction Withdraw(long amountInCents, DateTimeOffset occurredAtUtc)
    {
        EnsurePositiveAmount(amountInCents);

        if (BalanceInCents < amountInCents)
        {
            throw new InvalidOperationException("Insufficient funds.");
        }

        BalanceInCents -= amountInCents;
        var transaction = AccountTransaction.CreateWithdrawal(Id, amountInCents, BalanceInCents, occurredAtUtc);
        _transactions.Add(transaction);

        return transaction;
    }

    private static void EnsurePositiveAmount(long amountInCents)
    {
        if (amountInCents <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amountInCents), "Amount must be greater than zero.");
        }
    }
}
