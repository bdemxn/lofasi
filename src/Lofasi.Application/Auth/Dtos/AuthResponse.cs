namespace Lofasi.Application.Auth.Dtos;

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid UserId,
    Guid CustomerId,
    string Email);
