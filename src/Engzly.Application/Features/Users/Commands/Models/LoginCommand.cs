using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Application.Bases;
using Engzly.Application.Responses;
using MediatR;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed record LoginCommand( string Email , string Password ) : IRequest<Response<LoginResponse>>;

}
