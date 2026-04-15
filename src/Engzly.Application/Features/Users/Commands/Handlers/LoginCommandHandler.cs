using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Responses.UsersResponse;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Users.Commands.Handlers;

public class LoginCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ITokenService tokenService,
    ILogger<LoginCommandHandler> logger)
    : ResponseHandler,
      IRequestHandler<LoginCommand, Response<LoginResponse>>
{
    private const string InvalidEmailOrPassword = "Invalid Login";

    public async Task<Response<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Login attempt for Email: {Email}", request.Email);

        // Business Rule: Email must exist
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogWarning("Login failed: Email {Email} not found", request.Email);
            return BadRequest<LoginResponse>(InvalidEmailOrPassword);
        }

        if (user.Status == UserStatus.Pending)
        {
            logger.LogWarning("Login blocked: User {UserId} has not verified email", user.Id);
            return BadRequest<LoginResponse>("Email not verified. Please verify with the code we sent you.");
        }

        if (user.Status != UserStatus.Active)
        {
            logger.LogWarning("Login failed: User {UserId} status is {Status}", user.Id, user.Status);
            return BadRequest<LoginResponse>("Account is not Active");
        }

        // Check password
        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (result.IsLockedOut)
        {
            logger.LogWarning("Login failed: User {UserId} account is locked", user.Id);
            return BadRequest<LoginResponse>("Account is Locked. Try Again Later.");
        }

        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed: Invalid credentials for User {UserId}", user.Id);
            return BadRequest<LoginResponse>(InvalidEmailOrPassword);
        }

        // Generate tokens
        var jwtToken = await tokenService.GenerateJwtToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        logger.LogInformation("Login successful for User {UserId}", user.Id);

        var response = new LoginResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            AccessToken = jwtToken,
            RefreshToken = refreshToken
        };

        return Success(response, "Login is successful");
    }
}