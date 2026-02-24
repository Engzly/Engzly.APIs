using AutoMapper;
using Engzly.Application.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Interfaces;
using Engzly.Application.Responses;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class CreateUserCommandHandler(UserManager<User> _userManager, IMapper _mapper, ITokenService tokenService) : ResponseHandler,
     IRequestHandler<CreateUserCommand, Response<CreateUserResponse>>
    {

        public async Task<Response<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {

            var isUserNameExist = await _userManager.FindByNameAsync(request.UserName);
            if (isUserNameExist != null)
                return BadRequest<CreateUserResponse>("User Name Is Exist Before You Can't Add This Account Again ");

            //Logic to add user will be here
            var user = _mapper.Map<User>(request);
            user.SetLocation(request.Latitude, request.Longitude);
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {

                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest<CreateUserResponse>("Failed to create user", errors);
            }

            // Fix : Set user status to Active and confirm email
            user.Status = UserStatus.Active;
            user.EmailConfirmed = true;

            var accessToken = await tokenService.GenerateJwtToken(user);
            user.RefreshToken = tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);
            //----------------------------------------


            var response = new CreateUserResponse
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = user.RefreshToken
            };
            return Success(response, "User Created Successfully");
        }
    }
}
