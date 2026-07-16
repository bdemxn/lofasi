using System.Security.Cryptography;
using Lofasi.Application.Abstractions.Services;

namespace Lofasi.Infrastructure.Services;

public sealed class AccountNumberGenerator : IAccountNumberGenerator
{
    public string Generate(DateOnly creationDate)
    {
        var suffix = RandomNumberGenerator.GetInt32(0, 10_000);

        return $"ACC-{creationDate:yyyyMMdd}-{suffix:0000}";
    }
}
