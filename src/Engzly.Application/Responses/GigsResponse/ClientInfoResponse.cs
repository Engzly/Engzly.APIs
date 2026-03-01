using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engzly.Application.Responses.GigsResponse
{
    public sealed class ClientInfoResponse
    {
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string AvatarUrl { get; set; } = null!;
    }
}
