using Engzly.Application.Bases;
using MediatR;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed record CreateUserCommand(
        string FullName,
        string UserName,
        string Email,
        string PhoneNumber,
        string City,
        string Password
    ) : IRequest<Response<string>>;
}
