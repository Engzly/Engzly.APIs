using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed record DeleteUserCommand(Guid Id) : IRequest<Response<string>>;

}
