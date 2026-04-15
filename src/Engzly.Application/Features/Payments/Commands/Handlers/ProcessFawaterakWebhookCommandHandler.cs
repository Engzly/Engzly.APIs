using System.Text.Json;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Payments.Commands.Models;
using Engzly.Application.Interfaces.Payments;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Payments.Commands.Handlers
{
    public sealed class ProcessFawaterakWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentGateway paymentGateway,
        ILogger<ProcessFawaterakWebhookCommandHandler> logger)
        : ResponseHandler, IRequestHandler<ProcessFawaterakWebhookCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(ProcessFawaterakWebhookCommand request, CancellationToken ct)
        {
            if (!paymentGateway.TryVerifyWebhook(request.RawBody, request.Signature))
            {
                logger.LogWarning("Webhook rejected: signature verification failed");
                return Unauthorized<string>("Invalid signature.");
            }

            string? invoiceId;
            string? rawStatus;
            try
            {
                using var doc = JsonDocument.Parse(request.RawBody);
                var root = doc.RootElement;
                invoiceId = ExtractString(root, "invoice_id", "invoiceId");
                rawStatus = ExtractString(root, "invoice_status", "status");
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Webhook rejected: malformed JSON");
                return BadRequest<string>("Malformed payload.");
            }

            if (string.IsNullOrWhiteSpace(invoiceId))
                return BadRequest<string>("Missing invoice id.");

            var payment = (await unitOfWork.Payments.GetAllAsync(new PaymentByInvoiceSpec(invoiceId), ct)).FirstOrDefault();
            if (payment == null)
            {
                logger.LogWarning("Webhook: no payment for invoice {InvoiceId}", invoiceId);
                return NotFound<string>("Payment not found.");
            }

            await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
            {
                Id = Guid.NewGuid().ToString(),
                PaymentId = payment.Id,
                Type = PaymentEventType.WebhookReceived,
                RawPayload = Truncate(request.RawBody, 3800),
                OccurredAtUtc = DateTime.UtcNow
            }, ct);

            var mapped = paymentGateway.MapProviderStatus(rawStatus ?? string.Empty);

            if (payment.Status == PaymentStatus.Funded || payment.Status == PaymentStatus.Released)
            {
                logger.LogInformation(
                    "Webhook ignored (idempotent): payment {PaymentId} already in {Status}",
                    payment.Id, payment.Status);
                await unitOfWork.Payments.CompleteAsync(ct);
                return Success("Already processed.");
            }

            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                if (mapped == PaymentStatus.Funded)
                {
                    var gig = await unitOfWork.Gigs.GetByIdAsync(payment.GigId, ct);
                    if (gig == null)
                    {
                        await unitOfWork.RollbackTransactionAsync(ct);
                        return NotFound<string>("Gig missing for funded payment.");
                    }

                    payment.Status = PaymentStatus.Funded;
                    payment.FundedAtUtc = DateTime.UtcNow;
                    payment.UpdatedAtUtc = DateTime.UtcNow;
                    unitOfWork.Payments.Update(payment);

                    if (gig.Status == GigStatus.HelpersAssigned)
                    {
                        gig.Status = GigStatus.InProgress;
                        gig.LastModifiedOn = DateTime.UtcNow;
                        unitOfWork.Gigs.Update(gig);
                    }

                    await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
                    {
                        Id = Guid.NewGuid().ToString(),
                        PaymentId = payment.Id,
                        Type = PaymentEventType.Funded,
                        OccurredAtUtc = DateTime.UtcNow
                    }, ct);

                    logger.LogInformation("Webhook: payment {PaymentId} funded, gig {GigId} moved to InProgress", payment.Id, gig.Id);
                }
                else if (mapped == PaymentStatus.Failed || mapped == PaymentStatus.Expired)
                {
                    payment.Status = mapped;
                    payment.UpdatedAtUtc = DateTime.UtcNow;
                    unitOfWork.Payments.Update(payment);
                    await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
                    {
                        Id = Guid.NewGuid().ToString(),
                        PaymentId = payment.Id,
                        Type = PaymentEventType.Failed,
                        RawPayload = rawStatus,
                        OccurredAtUtc = DateTime.UtcNow
                    }, ct);
                }
                else
                {
                    logger.LogInformation(
                        "Webhook: payment {PaymentId} status unchanged, provider said {RawStatus}",
                        payment.Id, rawStatus);
                }

                await unitOfWork.Payments.CompleteAsync(ct);
                await unitOfWork.CommitTransactionAsync(ct);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }

            return Success("Processed.");
        }

        private static string? ExtractString(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (root.TryGetProperty(name, out var v))
                {
                    if (v.ValueKind == JsonValueKind.String) return v.GetString();
                    if (v.ValueKind == JsonValueKind.Number) return v.GetRawText();
                }
                if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object
                    && data.TryGetProperty(name, out var v2))
                {
                    if (v2.ValueKind == JsonValueKind.String) return v2.GetString();
                    if (v2.ValueKind == JsonValueKind.Number) return v2.GetRawText();
                }
            }
            return null;
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max);

        private sealed class PaymentByInvoiceSpec : BaseSpecification<Payment>
        {
            public PaymentByInvoiceSpec(string invoiceId) : base(p => p.ProviderInvoiceId == invoiceId) { }
        }
    }
}
