using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
    public sealed class StartGigCommand : IRequest<Response<StartGigResponse>>
    {
        public string GigId { get; set; } = null!;
    }
}
