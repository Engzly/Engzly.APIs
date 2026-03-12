using AutoMapper;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Services.Engzly.Application.Interfaces;
using Engzly.Application.Responses.UsersResponse;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class CreateUserCommandHandler(
        UserManager<User> userManager,
        IMapper mapper,
        ITokenService tokenService,
        IFileService fileService,
        ILogger<CreateUserCommandHandler> logger)
        : ResponseHandler,
          IRequestHandler<CreateUserCommand, Response<CreateUserResponse>>
    {
        public async Task<Response<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Attempting to create a new user with UserName {UserName}",
                request.UserName
            );

            // Business Rule: Username must be unique
            var isUserNameExist = await userManager.FindByNameAsync(request.UserName);
            if (isUserNameExist != null)
            {
                logger.LogWarning(
                    "User creation failed: UserName {UserName} already exists",
                    request.UserName
                );
                return BadRequest<CreateUserResponse>("User Name already exists, cannot create duplicate account.");
            }

            // Map request to User entity
            var user = mapper.Map<User>(request);

            // Create user in Identity
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                logger.LogWarning(
                    "Failed to create user {UserName}. Errors: {Errors}",
                    request.UserName,
                    string.Join(", ", errors)
                );
                return BadRequest<CreateUserResponse>("Failed to create user", errors);
            }

            // Set default user properties
            user.Status = UserStatus.Active;
            user.EmailConfirmed = true;

            // Generate tokens
            var accessToken = await tokenService.GenerateJwtToken(user);
            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            logger.LogInformation(
                "User {UserName} created successfully with Id {UserId}",
                request.UserName,
                user.Id
            );

            var response = new CreateUserResponse
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken
            };

            return Success(response, "User created successfully");
        }
    }
}