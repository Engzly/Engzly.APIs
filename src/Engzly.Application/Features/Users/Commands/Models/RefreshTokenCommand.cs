using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Application.Bases;
using Engzly.Application.Responses.UsersResponse;
using MediatR;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed record RefreshTokenCommand(string RefreshToken)
    : IRequest<Response<LoginResponse>>;
}
