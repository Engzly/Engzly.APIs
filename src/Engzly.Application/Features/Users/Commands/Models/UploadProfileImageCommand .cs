using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Application.Bases;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed class UploadProfileImageCommand : IRequest<Response<string>>
    {
        public IFormFile Image { get; set; } = null!;
    }
}
