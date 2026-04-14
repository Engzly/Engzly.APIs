namespace Engzly.Application.Interfaces.AI
{
    public interface ITaskEstimator
    {
        Task<TaskEstimate> EstimateAsync(
            TaskEstimationInput input,
            CancellationToken cancellationToken = default);
    }

    public sealed record TaskEstimationInput(
        string Title,
        string Description,
        string? CategoryId,
        int NumberOfTaskersNeeded);

    public sealed record TaskEstimate(
        string CategoryId,
        string? CategoryName,
        BudgetEstimate Budget,
        DurationEstimate Duration,
        string Method,
        int SampleSize,
        double Confidence);

    public sealed record BudgetEstimate(
        decimal Min,
        decimal Expected,
        decimal Max,
        string Currency);

    public sealed record DurationEstimate(
        int MinMinutes,
        int ExpectedMinutes,
        int MaxMinutes);
}
