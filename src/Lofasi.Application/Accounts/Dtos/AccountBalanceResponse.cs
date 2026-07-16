namespace Lofasi.Application.Accounts.Dtos;

public sealed record AccountBalanceResponse(
    string AccountNumber,
    decimal Balance,
    long BalanceInCents);
