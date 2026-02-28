using Engzly.Domain.Entities.Common;
using Engzly.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Domain.Entities.Identity
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Location Location { get; set; }
        public AccountType AccountType { get; set; }
        public string ProfileImageUrl { get; set; } = null!;
        public UserStatus Status { get; set; } = UserStatus.Pending;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        //OTP
        public string? OtpHash { get; set; }
        public DateTime? OtpExpiresAtUtc { get; set; }
        public int OtpAttempts { get; set; } = 0;

        public OtpPurpose? OtpPurpose { get; set; }
        public OtpChannel? OtpChannel { get; set; }

        public DateTime? OtpLastSentAtUtc { get; set; }

        // Navigation Property to Document(Photos )



        public void Delete()
        {
            Status = UserStatus.Deleted;
            LockoutEnabled = true;
            LockoutEnd = DateTimeOffset.MaxValue;
        }

        public void SetLocation(double latitude, double longitude)
        {
            Location = new Location(latitude, longitude);
        }

    }
}
