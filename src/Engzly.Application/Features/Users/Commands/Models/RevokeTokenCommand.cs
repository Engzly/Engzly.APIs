using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed record RevokeTokenCommand(string RefreshToken)
     : IRequest<Response<string>>;
}
