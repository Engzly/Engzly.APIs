using Engzly.Application.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class LogoutCommandHandler(UserManager<User> _userManager) : ResponseHandler, IRequestHandler<LogoutCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var user = _userManager.Users
                .FirstOrDefault(u => u.RefreshToken == request.refreshToken);

            if (user == null)
                return BadRequest<string>("Invalid refresh token");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded ? LoggedOutSuccessful("") : BadRequest<string>("Failed to logout");

        }
    }
}
