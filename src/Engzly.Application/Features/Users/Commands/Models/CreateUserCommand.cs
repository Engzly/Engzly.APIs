using Engzly.Application.Bases;
using Engzly.Application.Responses.UsersResponse;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed record CreateUserCommand(
        string FirstName,
        string LastName,
        string Email,
         string ProfileImageUrl,
        string PhoneNumber,
        string Password,
        AccountType AccountType,
        double Latitude,
        double Longitude
    ) : IRequest<Response<CreateUserResponse>>;
}
