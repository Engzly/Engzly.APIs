using Engzly.Domain.Entities.Identity;

namespace Engzly.Application.Interfaces.Authentication
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(User user);
        string GenerateRefreshToken();
    }
}
