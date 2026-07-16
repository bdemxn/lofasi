using Lofasi.Domain.Enums;

namespace Lofasi.Domain.Entities;

public sealed class AccountTransaction
{
    private AccountTransaction()
    {
    }

    private AccountTransaction(
        Guid bankAccountId,
        TransactionType type,
        long amountInCents,
        long balanceAfterTransactionInCents,
        DateTimeOffset occurredAtUtc)
    {
        if (bankAccountId == Guid.Empty)
        {
            throw new ArgumentException("Bank account id is required.", nameof(bankAccountId));
        }

        if (amountInCents <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amountInCents), "Transaction amount must be greater than zero.");
        }

        Id = Guid.NewGuid();
        BankAccountId = bankAccountId;
        Type = type;
        AmountInCents = amountInCents;
        BalanceAfterTransactionInCents = balanceAfterTransactionInCents;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid BankAccountId { get; private set; }

    public TransactionType Type { get; private set; }

    public long AmountInCents { get; private set; }

    public long BalanceAfterTransactionInCents { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public BankAccount? BankAccount { get; private set; }

    public static AccountTransaction CreateDeposit(
        Guid bankAccountId,
        long amountInCents,
        long balanceAfterTransactionInCents,
        DateTimeOffset occurredAtUtc)
    {
        return new AccountTransaction(
            bankAccountId,
            TransactionType.Deposit,
            amountInCents,
            balanceAfterTransactionInCents,
            occurredAtUtc);
    }

    public static AccountTransaction CreateWithdrawal(
        Guid bankAccountId,
        long amountInCents,
        long balanceAfterTransactionInCents,
        DateTimeOffset occurredAtUtc)
    {
        return new AccountTransaction(
            bankAccountId,
            TransactionType.Withdrawal,
            amountInCents,
            balanceAfterTransactionInCents,
            occurredAtUtc);
    }
}
