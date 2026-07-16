using Lofasi.Application.Auth.Dtos;

namespace Lofasi.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterCustomerRequest request, CancellationToken cancellationToken);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
