using Microsoft.AspNetCore.Identity;

namespace Lofasi.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid? CustomerId { get; set; }
}
