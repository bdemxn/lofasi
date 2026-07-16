using Lofasi.Domain.Enums;

namespace Lofasi.Application.Transactions.Dtos;

public sealed record TransactionResponse(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    long AmountInCents,
    DateTimeOffset Timestamp,
    decimal BalanceAfterTransaction,
    long BalanceAfterTransactionInCents);
