using System.Text.RegularExpressions;
using Lofasi.Infrastructure.Services;
using Xunit;

namespace Lofasi.UnitTests;

public sealed class AccountNumberGeneratorTests
{
    [Fact]
    public void Generate_ShouldReturnExpectedFormat()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var generator = new AccountNumberGenerator();

        var accountNumber = generator.Generate(date);

        Assert.Equal(17, accountNumber.Length);
        Assert.StartsWith("ACC-", accountNumber);
        Assert.Contains(date.ToString("yyyyMMdd"), accountNumber);
        Assert.Matches(new Regex($"^ACC-{date:yyyyMMdd}-\\d{{4}}$"), accountNumber);
    }

    [Fact]
    public void Generate_ShouldProvideBasicUniqueness()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var generator = new AccountNumberGenerator();

        var accountNumbers = Enumerable.Range(0, 100)
            .Select(_ => generator.Generate(date))
            .ToArray();

        Assert.True(accountNumbers.Distinct().Count() > 1);
    }
}
