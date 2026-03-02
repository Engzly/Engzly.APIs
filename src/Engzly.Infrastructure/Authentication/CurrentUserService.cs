using System.Security.Claims;
using Engzly.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;

namespace Engzly.Infrastructure.Authentication;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public CurrentUser GetCurrentUser()
    {
        var claimsPrincipal = httpContextAccessor.HttpContext!.User;
        var currentUser = new CurrentUser(
            claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)!.Value,
            claimsPrincipal.FindFirst(ClaimTypes.Email)!.Value,
            claimsPrincipal.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList()
        );
        return currentUser;
    }
}