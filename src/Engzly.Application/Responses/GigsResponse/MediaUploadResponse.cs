using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engzly.Application.Responses.GigsResponse
{
    public sealed class MediaUploadResponse
    {
        public Guid MediaId { get; set; }

        public string Url { get; set; } = null!;
    }
}
