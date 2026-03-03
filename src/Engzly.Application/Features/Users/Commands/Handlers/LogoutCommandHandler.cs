using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class LogoutCommandHandler(
        UserManager<User> userManager,
        ILogger<LogoutCommandHandler> logger)
        : ResponseHandler, IRequestHandler<LogoutCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Logout attempt with RefreshToken {RefreshToken}", request.refreshToken);

            // Business Rule: Refresh token must exist
            var user = userManager.Users
                .FirstOrDefault(u => u.RefreshToken == request.refreshToken);

            if (user == null)
            {
                logger.LogWarning("Logout failed: Invalid refresh token {RefreshToken}", request.refreshToken);
                return BadRequest<string>("Invalid refresh token");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                logger.LogInformation("User {UserId} logged out successfully", user.Id);
                return LoggedOutSuccessful("");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to logout User {UserId}. Errors: {Errors}", user.Id, errors);
                return BadRequest<string>("Failed to logout");
            }
        }
    }
}