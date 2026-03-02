using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed record LogoutCommand(string refreshToken) : IRequest<Response<string>>;
}
