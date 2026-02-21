using AutoMapper;
using Engzly.Application.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class CreateUserCommandHandler(UserManager<User> _userManager, IMapper _mapper) : ResponseHandler,
     IRequestHandler<CreateUserCommand, Response<string>>
    {
        
        public async Task<Response<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var isEmailExist = await _userManager.FindByEmailAsync(request.Email);
            
            if (isEmailExist != null)
                return BadRequest<string>("Email Is Exist Before You Can't Add This Account Again ");

            var isUserNameExist = await _userManager.FindByNameAsync(request.UserName);
            if (isUserNameExist != null)
                return BadRequest<string>("User Name Is Exist Before You Can't Add This Account Again ");

            //Logic to add user will be here
            var user = _mapper.Map<User>(request);
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {

                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest<string>("Failed to create user", errors);
            }

            // Fix : Set user status to Active and confirm email
            user.Status = UserStatus.Active;     
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            //----------------------------------------

            return new Response<string>
            {
                Data = user.Id,
                Succeeded = true,
                StatusCode = 201,
                Message = "User created successfully"
            };
        }
    }
}
