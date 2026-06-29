namespace Engzly.Application.Responses.GigsResponse
{
    public sealed record TaskerEarningsResponse(
    decimal Today,
    decimal ThisWeek,
    decimal ThisMonth,
    decimal Total,
    string Currency);
}
