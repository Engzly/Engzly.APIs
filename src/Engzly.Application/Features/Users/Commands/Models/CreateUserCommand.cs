using Engzly.Application.Bases;
using Engzly.Application.Responses;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed record CreateUserCommand(
        string FullName,
        string UserName,
        string Email,
        string PhoneNumber,
        string Password,
        AccountType AccountType,
        double Latitude,
        double Longitude
    ) : IRequest<Response<CreateUserResponse>>;
}
