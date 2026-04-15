using System.Security.Cryptography;
using System.Text;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Notifications;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Engzly.Infrastructure.Authorization.OtpSecurity
{
    public class OtpService(
        UserManager<User> userManager,
        IEmailSender email,
        IUnitOfWork unitOfWork,
        ILogger<OtpService> logger)
        : IOtpService
    {
        private const int MaxAttempts = 5;
        private const int CooldownSeconds = 45;
        private const int DailySendQuota = 8;
        private const int ExpiryMinutes = 5;

        public async Task<(bool ok, string? error)> SendOtpAsync(
            string userId, OtpPurpose purpose, OtpChannel channel, string destination, CancellationToken ct)
        {
            // `destination` is intentionally ignored — channel target is derived from the user entity
            // so a client cannot steer OTPs at a victim's address.
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("OTP send rejected: user {UserId} not found", userId);
                return (false, "User not found.");
            }

            if (channel != OtpChannel.Email)
            {
                logger.LogWarning("OTP send rejected: unsupported channel {Channel} for user {UserId}", channel, userId);
                return (false, "Unsupported OTP channel.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                logger.LogWarning("OTP send rejected: user {UserId} has no email on file", userId);
                return (false, "User has no email on file.");
            }

            var nowUtc = DateTime.UtcNow;

            // Reset daily quota window if last reset was on a previous UTC day.
            if (user.OtpSendQuotaResetAtUtc == null || user.OtpSendQuotaResetAtUtc.Value.Date < nowUtc.Date)
            {
                user.OtpSendCountToday = 0;
                user.OtpSendQuotaResetAtUtc = nowUtc.Date;
            }

            if (user.OtpSendCountToday >= DailySendQuota)
            {
                logger.LogWarning("OTP send rejected: daily quota exhausted for user {UserId}", userId);
                return (false, "Daily OTP send limit reached. Please try again tomorrow.");
            }

            if (user.OtpLastSentAtUtc.HasValue && user.OtpLastSentAtUtc.Value.AddSeconds(CooldownSeconds) > nowUtc)
            {
                logger.LogInformation("OTP send rejected: cooldown active for user {UserId}", userId);
                return (false, "Please wait before requesting another OTP.");
            }

            var code = GenerateNumericCode(6);

            user.OtpHash = Hash(code);
            user.OtpExpiresAtUtc = nowUtc.AddMinutes(ExpiryMinutes);
            user.OtpAttempts = 0;
            user.OtpPurpose = purpose;
            user.OtpChannel = channel;
            user.OtpLastSentAtUtc = nowUtc;
            user.OtpSendCountToday += 1;

            var upd = await userManager.UpdateAsync(user);
            if (!upd.Succeeded)
            {
                logger.LogError("OTP send failed: could not persist OTP state for user {UserId}", userId);
                return (false, "Failed to save OTP on user.");
            }

            var msg = $"Your verification code is: {code}. It expires in {ExpiryMinutes} minutes.";
            await email.SendAsync(user.Email, "Your Engzly verification code", msg, ct);

            logger.LogInformation(
                "OTP sent: user {UserId} purpose {Purpose} channel {Channel} (quotaUsed {Used}/{Total})",
                userId, purpose, channel, user.OtpSendCountToday, DailySendQuota);

            return (true, null);
        }

        public async Task<(bool ok, string? error)> VerifyOtpAsync(
            string userId, OtpPurpose purpose, OtpChannel channel, string code, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("OTP verify rejected: user {UserId} not found", userId);
                return (false, "User not found.");
            }

            if (user.OtpHash == null || user.OtpExpiresAtUtc == null)
            {
                logger.LogWarning("OTP verify rejected: no OTP on record for user {UserId}", userId);
                return (false, "No OTP requested.");
            }

            if (user.OtpExpiresAtUtc <= DateTime.UtcNow)
            {
                await ClearOtpStateAsync(user);
                logger.LogWarning("OTP verify rejected: expired for user {UserId}", userId);
                return (false, "OTP expired.");
            }

            if (user.OtpPurpose != purpose || user.OtpChannel != channel)
            {
                user.OtpAttempts += 1;
                await userManager.UpdateAsync(user);
                logger.LogWarning("OTP verify rejected: purpose/channel mismatch for user {UserId}", userId);
                return (false, "OTP purpose/channel mismatch.");
            }

            if (user.OtpAttempts >= MaxAttempts)
            {
                await ClearOtpStateAsync(user);
                logger.LogWarning("OTP verify lockout: user {UserId} exceeded attempt limit", userId);
                return (false, "Too many attempts. Please request a new code.");
            }

            if (!SlowEquals(user.OtpHash, Hash(code)))
            {
                user.OtpAttempts += 1;
                await userManager.UpdateAsync(user);
                logger.LogWarning(
                    "OTP verify rejected: invalid code for user {UserId} (attempt {Attempt}/{Max})",
                    userId, user.OtpAttempts, MaxAttempts);
                return (false, "Invalid code.");
            }

            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                user.OtpHash = null;
                user.OtpExpiresAtUtc = null;
                user.OtpAttempts = 0;
                user.OtpPurpose = null;
                user.OtpChannel = null;

                if (purpose == OtpPurpose.VerifyAccount)
                {
                    user.Status = UserStatus.Active;
                    user.EmailConfirmed = true;
                }

                var saveResult = await userManager.UpdateAsync(user);
                if (!saveResult.Succeeded)
                {
                    await unitOfWork.RollbackTransactionAsync(ct);
                    logger.LogError("OTP verify failed: could not persist consume for user {UserId}", userId);
                    return (false, "Failed to consume OTP.");
                }

                await unitOfWork.CommitTransactionAsync(ct);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }

            logger.LogInformation("OTP verified: user {UserId} purpose {Purpose}", userId, purpose);
            return (true, null);
        }

        private async Task ClearOtpStateAsync(User user)
        {
            user.OtpHash = null;
            user.OtpExpiresAtUtc = null;
            user.OtpAttempts = 0;
            user.OtpPurpose = null;
            user.OtpChannel = null;
            await userManager.UpdateAsync(user);
        }

        private static string GenerateNumericCode(int length)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);
            foreach (var b in bytes) sb.Append((b % 10).ToString());
            return sb.ToString();
        }

        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        private static bool SlowEquals(string a, string b)
        {
            if (a.Length != b.Length) return false;
            var diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
