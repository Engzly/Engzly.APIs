using AutoMapper;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Interfaces;
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
        IOtpService otpService,
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

            var isUserNameExist = await userManager.FindByNameAsync(request.UserName);
            if (isUserNameExist != null)
            {
                logger.LogWarning(
                    "User creation failed: UserName {UserName} already exists",
                    request.UserName
                );
                return BadRequest<CreateUserResponse>("User Name already exists, cannot create duplicate account.");
            }

            var user = mapper.Map<User>(request);
            user.Status = UserStatus.Pending;
            user.EmailConfirmed = false;

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

            logger.LogInformation(
                "User {UserName} created in Pending state with Id {UserId}",
                request.UserName,
                user.Id
            );

            var (otpOk, otpError) = await otpService.SendOtpAsync(
                user.Id,
                OtpPurpose.VerifyAccount,
                OtpChannel.Email,
                user.Email!,
                cancellationToken);

            if (!otpOk)
            {
                logger.LogWarning(
                    "User {UserId} created but verification OTP failed to send: {Error}",
                    user.Id, otpError);
            }


            string role = request.AccountType switch
            {
                AccountType.Helper => "Helper",
                AccountType.Admin => "Admin",
                _ => "Client"
            };

            await userManager.AddToRoleAsync(user, role);


            var response = new CreateUserResponse
            {
                UserId = user.Id,
                RequiresEmailVerification = true,
                Role = role
            };

            return Success(
                response,
                "Account created. A verification code has been sent to your email.");
        }
    }
}
