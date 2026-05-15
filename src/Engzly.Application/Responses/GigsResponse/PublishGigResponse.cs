namespace Engzly.Application.Responses.GigsResponse
{
    public sealed record PublishGigResponse
    (
        string GigId,
        string ClientName,
        string Title,
        string Description,
        string TaskStartDate,
        string DueDate,
        decimal Budget
    );
}
