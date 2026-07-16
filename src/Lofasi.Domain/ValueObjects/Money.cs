namespace Lofasi.Domain.ValueObjects;

public static class Money
{
    public const int CentsPerUnit = 100;

    public static long ToCents(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Money amount cannot be negative.");
        }

        var cents = amount * CentsPerUnit;

        if (cents != decimal.Truncate(cents))
        {
            throw new ArgumentException("Money amount cannot have more than two decimal places.", nameof(amount));
        }

        if (cents > long.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Money amount is too large.");
        }

        return (long)cents;
    }

    public static decimal FromCents(long cents)
    {
        return cents / (decimal)CentsPerUnit;
    }
}
