using System.Security.Claims;
using Engzly.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;

namespace Engzly.Infrastructure.Authentication;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public CurrentUser GetCurrentUser()
    {
        //var claimsPrincipal = httpContextAccessor.HttpContext!.User;
        //var currentUser = new CurrentUser(
        //    claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)!.Value,
        //    claimsPrincipal.FindFirst(ClaimTypes.Email)!.Value,
        //    claimsPrincipal.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList()
        //);
        //return currentUser;

        var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No active HTTP context.");

        var claimsPrincipal = httpContext.User;

        if (claimsPrincipal?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var id = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID claim is missing.");

        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value
            ?? throw new UnauthorizedAccessException("Email claim is missing.");

        var accountType = claimsPrincipal.FindFirst("accountType")?.Value;


        var roles = claimsPrincipal.FindAll(ClaimTypes.Role)
            .Select(r => r.Value)
            .ToList();

        return new CurrentUser(id, email, accountType, roles);
    }
}