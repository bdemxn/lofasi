using Lofasi.Domain.Enums;

namespace Lofasi.Application.Auth.Dtos;

public sealed record RegisterCustomerRequest(
    string Email,
    string Password,
    string FullName,
    DateOnly DateOfBirth,
    Gender Gender,
    decimal MonthlyIncome);
