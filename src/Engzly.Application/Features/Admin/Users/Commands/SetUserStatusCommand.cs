using Engzly.Application.Common.Bases;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Admin.Users.Commands
{
    public sealed record BlockUserCommand(string UserId)
        : IRequest<Response<string>>;

    public sealed record UnblockUserCommand(string UserId)
        : IRequest<Response<string>>;

    public sealed class BlockUserCommandHandler(UserManager<User> _userManager)
        : ResponseHandler, IRequestHandler<BlockUserCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            BlockUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
                return NotFound<string>("User not found");

            user.Status = UserStatus.Blocked;
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest<string>(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Success(user.Id, "User blocked");
        }
    }

    public sealed class UnblockUserCommandHandler(UserManager<User> _userManager)
        : ResponseHandler, IRequestHandler<UnblockUserCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            UnblockUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
                return NotFound<string>("User not found");

            user.Status = UserStatus.Active;
            user.LockoutEnd = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest<string>(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Success(user.Id, "User unblocked");
        }
    }
}
