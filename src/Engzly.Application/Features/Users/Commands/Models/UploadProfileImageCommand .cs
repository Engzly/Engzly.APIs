using Engzly.Application.Common.Bases;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Features.Users.Commands.Models
{
    public sealed class UploadProfileImageCommand : IRequest<Response<string>>
    {
        public IFormFile? Image { get; set; }
        public string UserId { get; set; }
    }
}
