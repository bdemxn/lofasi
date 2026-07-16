using System.Security.Claims;
using Lofasi.Application.Abstractions.Authentication;

namespace Lofasi.API.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : Guid.Empty;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true && UserId != Guid.Empty;
}
