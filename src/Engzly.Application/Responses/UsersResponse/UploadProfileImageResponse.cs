using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Responses.UsersResponse
{
    public sealed class UploadProfileImageResponse
    {
        public IFormFile Image { get; set; } = null!;
    }
}
