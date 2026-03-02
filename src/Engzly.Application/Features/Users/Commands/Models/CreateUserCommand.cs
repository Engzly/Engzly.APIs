using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.UsersResponse;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed record CreateUserCommand(
         string FirstName,
        string LastName,
        string UserName,
        string Email,
        string PhoneNumber,
        string Password,
        AccountType AccountType,
        //double Latitude,  // If you want to include location in your request, you can uncomment these lines and add them to your handler
        //double Longitude, // If you want to include location in your request, you can uncomment these lines and add them to your handler
        string City,
        IFormFile? ProfileImage
    ) : IRequest<Response<CreateUserResponse>>;
}
