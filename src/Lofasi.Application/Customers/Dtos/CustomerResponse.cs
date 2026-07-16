using Lofasi.Domain.Enums;

namespace Lofasi.Application.Customers.Dtos;

public sealed record CustomerResponse(
    Guid Id,
    Guid UserId,
    string FullName,
    DateOnly DateOfBirth,
    Gender Gender,
    decimal MonthlyIncome,
    long MonthlyIncomeInCents,
    DateTimeOffset CreatedAtUtc);
