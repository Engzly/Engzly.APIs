using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Responses.UsersResponse;
using Engzly.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class RefreshTokenCommandHandler(
       UserManager<User> userManager,
       ITokenService tokenService,
       ILogger<RefreshTokenCommandHandler> logger)
       : ResponseHandler,
         IRequestHandler<RefreshTokenCommand, Response<LoginResponse>>
    {
        public async Task<Response<LoginResponse>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Attempting refresh token for token {RefreshToken}", request.RefreshToken);

            // Business Rule: Refresh token must exist
            var user = userManager.Users.FirstOrDefault(u =>
                u.RefreshToken == request.RefreshToken);

            if (user == null)
            {
                logger.LogWarning("Refresh token failed: Invalid token {RefreshToken}", request.RefreshToken);
                return BadRequest<LoginResponse>("Invalid Refresh Token");
            }

            // Business Rule: Refresh token must not be expired
            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                logger.LogWarning(
                    "Refresh token failed: Token expired for User {UserId}",
                    user.Id
                );
                return BadRequest<LoginResponse>("Refresh Token Expired");
            }

            // Generate new tokens
            var newJwt = await tokenService.GenerateJwtToken(user);
            var newRefresh = tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefresh;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            logger.LogInformation("Refresh token successful for User {UserId}", user.Id);

            return Success(new LoginResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                AccessToken = newJwt,
                RefreshToken = newRefresh
            }, "Token Refreshed Successfully");
        }
    }
}