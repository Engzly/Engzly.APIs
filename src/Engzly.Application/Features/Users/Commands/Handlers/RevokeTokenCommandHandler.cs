using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class RevokeTokenCommandHandler(
        UserManager<User> userManager,
        ILogger<RevokeTokenCommandHandler> logger)
        : ResponseHandler,
            IRequestHandler<RevokeTokenCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            RevokeTokenCommand request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Attempting to revoke refresh token {RefreshToken}", request.RefreshToken);

            // Business Rule: Refresh token must exist
            var user = userManager.Users.FirstOrDefault(u =>
                u.RefreshToken == request.RefreshToken);

            if (user == null)
            {
                logger.LogWarning("Revoke token failed: Invalid refresh token {RefreshToken}", request.RefreshToken);
                return BadRequest<string>("Invalid Refresh Token");
            }

            // Clear token and expiry
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await userManager.UpdateAsync(user);

            logger.LogInformation("Refresh token revoked successfully for User {UserId}", user.Id);

            return Success(user.Id, "Token Revoked Successfully");
        }
    }
}