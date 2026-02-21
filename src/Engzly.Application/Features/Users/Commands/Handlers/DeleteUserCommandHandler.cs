using AutoMapper;
using Engzly.Application.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class DeleteUserCommandHandler(UserManager<User> _userManager, IMapper _mapper) : ResponseHandler,
     IRequestHandler<DeleteUserCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {

            // Check User Exist Or  Not 
            var _UserExist = await _userManager.FindByIdAsync(request.Id.ToString());
            if (_UserExist is null)
                return NotFound<string>($"User WIth Id {request.Id} IS Not Exist ");
            // Logic To Delete User
            var _deleteUser = await _userManager.DeleteAsync(_UserExist);
            if (!_deleteUser.Succeeded)
                return BadRequest<string>("Deleted Faild ");
            return Deleted("");

        }

    }
}


