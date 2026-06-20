using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Engzly.Application.Interfaces.Payments;
using Engzly.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Engzly.Infrastructure.Payments
{
    public sealed class FawaterakPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _http;
        private readonly FawaterakOptions _options;
        private readonly ILogger<FawaterakPaymentGateway> _logger;

        public FawaterakPaymentGateway(
            HttpClient http,
            IOptions<FawaterakOptions> options,
            ILogger<FawaterakPaymentGateway> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<CreateInvoiceResult> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken ct)
        {
            var body = new
            {
                cartTotal = request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                currency = request.Currency,
                customer = new
                {
                    first_name = request.CustomerFirstName,
                    last_name = request.CustomerLastName,
                    email = request.CustomerEmail,
                    phone = request.CustomerPhone ?? string.Empty,
                    address = "N/A"
                },
                redirectionUrls = new
                {
                    successUrl = _options.SuccessUrl,
                    failUrl = _options.FailUrl,
                    pendingUrl = _options.PendingUrl
                },
                cartItems = new[]
                {
                    new
                    {
                        name = request.GigTitle,
                        price = request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                        quantity = "1"
                    }
                },
                payLoad = new
                {
                    paymentId = request.PaymentId,
                    gigId = request.GigId
                }
            };

            using var message = new HttpRequestMessage(HttpMethod.Post, "/api/v2/createInvoiceLink")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            message.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
            message.Headers.Add("vendorKey", _options.ProviderKey);

            try
            {
                using var response = await _http.SendAsync(message, ct);
                var raw = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Fawaterak createInvoiceLink failed: {Status} {Body}",
                        response.StatusCode, raw);
                    return new CreateInvoiceResult { Ok = false, Error = $"Gateway {(int)response.StatusCode}" };
                }

                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                if (!root.TryGetProperty("status", out var status) || status.GetString() != "success")
                {
                    return new CreateInvoiceResult { Ok = false, Error = "Gateway returned non-success status" };
                }

                var data = root.GetProperty("data");
                return new CreateInvoiceResult
                {
                    Ok = true,
                    ProviderInvoiceId = data.TryGetProperty("invoiceId", out var id) ? id.ToString() : null,
                    ProviderInvoiceKey = data.TryGetProperty("invoiceKey", out var key) ? key.GetString() : null,
                    PaymentUrl = data.TryGetProperty("url", out var url) ? url.GetString() : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fawaterak createInvoiceLink threw");
                return new CreateInvoiceResult { Ok = false, Error = ex.Message };
            }
        }

        public async Task<InvoiceStatusResult> GetInvoiceStatusAsync(string providerInvoiceId, CancellationToken ct)
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/getInvoiceData/{providerInvoiceId}");
            message.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
            message.Headers.Add("vendorKey", _options.ProviderKey);

            try
            {
                using var response = await _http.SendAsync(message, ct);
                var raw = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new InvoiceStatusResult { Ok = false, Error = $"Gateway {(int)response.StatusCode}" };
                }

                using var doc = JsonDocument.Parse(raw);
                var data = doc.RootElement.TryGetProperty("data", out var d) ? d : doc.RootElement;
                var rawStatus = data.TryGetProperty("invoice_status", out var s) ? s.GetString() : null;

                return new InvoiceStatusResult
                {
                    Ok = true,
                    RawStatus = rawStatus,
                    RawPayload = raw,
                    Status = MapProviderStatus(rawStatus ?? string.Empty)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fawaterak getInvoiceData threw");
                return new InvoiceStatusResult { Ok = false, Error = ex.Message };
            }
        }

        // FIXED: Fawaterak does NOT send a signature header. It sends a "hashKey" field
        // INSIDE the JSON body, computed as HMAC-SHA256 over
        // "InvoiceId={id}&InvoiceKey={key}&PaymentMethod={method}" using the Vendor Key.
        // The old implementation hashed the raw body against a header that never arrives,
        // so every real webhook from Fawaterak would have been rejected with 401.
        public bool TryVerifyWebhook(string rawBody, string? signatureHeader)
        {
            if (string.IsNullOrWhiteSpace(_options.ProviderKey))
            {
                _logger.LogWarning("Webhook verification skipped: ProviderKey (vendor key) is not configured");
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;

                if (!root.TryGetProperty("hashKey", out var hashKeyEl) || hashKeyEl.ValueKind != JsonValueKind.String)
                {
                    _logger.LogWarning("Webhook rejected: missing hashKey field in body");
                    return false;
                }

                var receivedHash = hashKeyEl.GetString();
                if (string.IsNullOrWhiteSpace(receivedHash))
                    return false;

                var invoiceId = root.TryGetProperty("invoice_id", out var idEl) ? idEl.ToString() : string.Empty;
                var invoiceKey = root.TryGetProperty("invoice_key", out var keyEl) ? keyEl.GetString() ?? string.Empty : string.Empty;
                var paymentMethod = root.TryGetProperty("payment_method", out var pmEl) ? pmEl.GetString() ?? string.Empty : string.Empty;

                var queryParam = $"InvoiceId={invoiceId}&InvoiceKey={invoiceKey}&PaymentMethod={paymentMethod}";

                var key = Encoding.UTF8.GetBytes(_options.ProviderKey);
                using var hmac = new HMACSHA256(key);
                var computed = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(queryParam))).ToLowerInvariant();

                var isValid = CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(computed),
                    Encoding.UTF8.GetBytes(receivedHash.Trim().ToLowerInvariant()));

                if (!isValid)
                {
                    _logger.LogWarning("Webhook rejected: hashKey mismatch for invoice {InvoiceId}", invoiceId);
                }

                return isValid;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Webhook rejected: malformed JSON during signature check");
                return false;
            }
        }

        public PaymentStatus MapProviderStatus(string providerStatus) => providerStatus.Trim().ToLowerInvariant() switch
        {
            "paid" => PaymentStatus.Funded,
            "successful" => PaymentStatus.Funded,
            "success" => PaymentStatus.Funded,
            "refunded" => PaymentStatus.Refunded,
            "failed" => PaymentStatus.Failed,
            "canceled" => PaymentStatus.Failed,
            "cancelled" => PaymentStatus.Failed,
            "expired" => PaymentStatus.Expired,
            "pending" => PaymentStatus.AwaitingFunding,
            _ => PaymentStatus.Pending
        };
    }
}