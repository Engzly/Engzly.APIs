using Engzly.Domain.Enums;

namespace Engzly.Application.Responses.GigsResponse
{
    public sealed class TaskListItemResponse
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CategoryId { get; set; } = null!;
        public string? CategoryName { get; set; }
        public decimal Budget { get; set; }
        public int NumberOfTaskersNeeded { get; set; }
        public GigStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string OwnerId { get; set; } = null!;
    }
}
