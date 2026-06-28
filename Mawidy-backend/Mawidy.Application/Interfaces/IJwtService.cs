using Mawidy.Domain.Entities;

namespace Mawidy.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
