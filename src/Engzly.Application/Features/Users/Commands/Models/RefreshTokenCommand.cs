using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.UsersResponse;
using MediatR;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed record RefreshTokenCommand(string RefreshToken)
    : IRequest<Response<LoginResponse>>;
}
