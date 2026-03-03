using Engzly.Domain.Enums;

namespace Engzly.Application.Interfaces
{
     public interface IOtpService
    {
        Task<(bool ok, string? error)> SendOtpAsync(
            string userId,
            OtpPurpose purpose,
            OtpChannel channel,
            string destination,
            CancellationToken ct);

        Task<(bool ok, string? error)> VerifyOtpAsync(
            string userId,
            OtpPurpose purpose,
            OtpChannel channel,
            string code,
            CancellationToken ct);
    }
}
