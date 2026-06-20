using Engzly.Application.Features.Payments.Commands.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Engzly.Infrastructure.Payments
{
    public sealed class PaymentReconciliationHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentReconciliationHostedService> logger)
        : BackgroundService
    {
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _minAge = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _maxAge = TimeSpan.FromDays(2);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation(
                "PaymentReconciliation started (interval={Interval}, minAge={MinAge})",
                _interval, _minAge);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "PaymentReconciliation tick failed");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task RunOnceAsync(CancellationToken ct)
        {
            // ? ??????? ?????? ???: ????? ?? await using ???????? CreateAsyncScope
            await using var scope = scopeFactory.CreateAsyncScope();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var now = DateTime.UtcNow;
            var olderThan = now - _minAge;
            var youngerThan = now - _maxAge;

            var stale = await unitOfWork.Payments.GetAllAsync(new StalePendingPaymentsSpec(olderThan, youngerThan), ct);
            if (stale.Count == 0) return;

            logger.LogInformation("PaymentReconciliation: {Count} stale pending payment(s)", stale.Count);

            foreach (var payment in stale)
            {
                if (ct.IsCancellationRequested) break;
                try
                {
                    await mediator.Send(new ReconcilePaymentCommand { PaymentId = payment.Id }, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "PaymentReconciliation: payment {PaymentId} failed", payment.Id);
                }
            }
        }

        private sealed class StalePendingPaymentsSpec : BaseSpecification<Payment>
        {
            public StalePendingPaymentsSpec(DateTime olderThan, DateTime youngerThan)
                : base(p => p.Status == PaymentStatus.AwaitingFunding
                            && p.UpdatedAtUtc <= olderThan
                            && p.UpdatedAtUtc >= youngerThan)
            {
            }
        }
    }
}