using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lofasi.Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lofasi.Infrastructure.Identity;

public sealed class JwtTokenService(IOptions<JwtOptions> jwtOptions) : IJwtTokenService
{
    public JwtTokenResult CreateToken(Guid userId, string email)
    {
        var options = jwtOptions.Value;
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(options.ExpirationMinutes);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new JwtTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
