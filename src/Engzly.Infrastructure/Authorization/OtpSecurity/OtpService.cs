using System.Security.Cryptography;
using System.Text;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Notifications;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Infrastructure.Authorization.OtpSecurity
{
  

   public class OtpService(UserManager<User> userManager, IEmailSender email, IWhatsAppSender whatsApp)
       : IOtpService
   {
       public async Task<(bool ok, string? error)> SendOtpAsync(
            string userId, OtpPurpose purpose, OtpChannel channel, string destination, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found.");

            // resend cooldown 45 seconds
            if (user.OtpLastSentAtUtc.HasValue && user.OtpLastSentAtUtc.Value.AddSeconds(45) > DateTime.UtcNow)
                return (false, "Please wait before requesting another OTP.");

            var code = GenerateNumericCode(6);

            user.OtpHash = Hash(code);
            user.OtpExpiresAtUtc = DateTime.UtcNow.AddMinutes(5);
            user.OtpAttempts = 0;
            user.OtpPurpose = purpose;
            user.OtpChannel = channel;
            user.OtpLastSentAtUtc = DateTime.UtcNow;

            var upd = await userManager.UpdateAsync(user);
            if (!upd.Succeeded)
                return (false, "Failed to save OTP on user.");

            var msg = $"Your code is: {code}. It expires in 5 minutes.";

            if (channel == OtpChannel.Email)
                await email.SendAsync(destination, "OTP Code", msg, ct);
            else
                await whatsApp.SendAsync(destination, msg, ct);

            return (true, null);
        }

        public async Task<(bool ok, string? error)> VerifyOtpAsync(
            string userId, OtpPurpose purpose, OtpChannel channel, string code, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found.");

            if (user.OtpHash == null || user.OtpExpiresAtUtc == null)
                return (false, "No OTP requested.");

            if (user.OtpExpiresAtUtc <= DateTime.UtcNow)
                return (false, "OTP expired.");

            if (user.OtpAttempts >= 5)
                return (false, "Too many attempts.");

            if (user.OtpPurpose != purpose || user.OtpChannel != channel)
                return (false, "OTP purpose/channel mismatch.");

            user.OtpAttempts++;

            if (!SlowEquals(user.OtpHash, Hash(code)))
            {
                await userManager.UpdateAsync(user);
                return (false, "Invalid code.");
            }

            // consume OTP
            user.OtpHash = null;
            user.OtpExpiresAtUtc = null;
            user.OtpAttempts = 0;
            user.OtpPurpose = null;
            user.OtpChannel = null;

            await userManager.UpdateAsync(user);
            return (true, null);
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
