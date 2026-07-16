using Lofasi.Domain.Enums;

namespace Lofasi.Domain.Entities;

public sealed class Customer
{
    private readonly List<BankAccount> _accounts = [];

    private Customer()
    {
        FullName = string.Empty;
    }

    public Customer(
        Guid userId,
        string fullName,
        DateOnly dateOfBirth,
        Gender gender,
        long monthlyIncomeInCents,
        DateTimeOffset createdAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (monthlyIncomeInCents < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthlyIncomeInCents), "Monthly income cannot be negative.");
        }

        Id = Guid.NewGuid();
        UserId = userId;
        FullName = fullName.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender;
        MonthlyIncomeInCents = monthlyIncomeInCents;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string FullName { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public Gender Gender { get; private set; }

    public long MonthlyIncomeInCents { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<BankAccount> Accounts => _accounts.AsReadOnly();
}
