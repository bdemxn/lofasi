namespace Lofasi.Application.Accounts.Dtos;

public sealed record AccountResponse(
    Guid Id,
    string AccountNumber,
    decimal Balance,
    long BalanceInCents,
    DateTimeOffset CreatedAtUtc);
