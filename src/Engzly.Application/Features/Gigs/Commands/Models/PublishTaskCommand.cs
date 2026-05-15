using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
    public sealed class PublishTaskCommand : IRequest<Response<PublishGigResponse>>
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public string? CategoryId { get; set; }
        public int NumberOfTaskersNeeded { get; set; }
        public List<Guid>? MediaIds { get; set; } = new();

        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Budget { get; set; }


    }
}

// Category Id ? 

