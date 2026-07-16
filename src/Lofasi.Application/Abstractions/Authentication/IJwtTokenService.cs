namespace Lofasi.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(Guid userId, string email);
}

public sealed record JwtTokenResult(string AccessToken, DateTimeOffset ExpiresAtUtc);
