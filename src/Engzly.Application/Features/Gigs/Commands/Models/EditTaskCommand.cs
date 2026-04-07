using System.Text.Json.Serialization;
using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
    public sealed class EditTaskCommand : IRequest<Response<string>>
    {
        [JsonIgnore]
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public double Latitude { get; init; }
        public double Longitude { get; init; }

        public string CategoryId { get; set; } = null!;
        public List<string> MediaUrls { get; set; } = new();
        public int NumberOfTaskersNeeded { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Budget { get; set; }

    }





}