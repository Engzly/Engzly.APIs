using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Notifications;
using Engzly.Domain.Entities.Verification;
using Engzly.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Domain.Entities.Identity
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string City { get; set; }

        //public Location Location { get; set; }
        public AccountType AccountType { get; set; }
        public string? ProfileImageUrl { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Pending;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();

        public bool IsIdentityVerified { get; set; } = false;
        public IdentityVerification? IdentityVerification { get; set; }

        //OTP
        public string? OtpHash { get; set; }
        public DateTime? OtpExpiresAtUtc { get; set; }
        public int OtpAttempts { get; set; } = 0;

        public OtpPurpose? OtpPurpose { get; set; }
        public OtpChannel? OtpChannel { get; set; }

        public DateTime? OtpLastSentAtUtc { get; set; }

        public int OtpSendCountToday { get; set; } = 0;
        public DateTime? OtpSendQuotaResetAtUtc { get; set; }

        // Navigation Property to Document(Photos )

        public ICollection<Gig> OwnedGigs { get; set; } = new HashSet<Gig>();
        public ICollection<GigAssignment> GigsAssignments { get; set; } = new HashSet<GigAssignment>();
        public ICollection<GigAssignment> AssignmentsAsClient { get; set; } = new HashSet<GigAssignment>();

        public void Delete()
        {
            Status = UserStatus.Deleted;
            LockoutEnabled = true;
            LockoutEnd = DateTimeOffset.MaxValue;
        }

        //public void SetLocation(double latitude, double longitude)
        //{
        //    Location = new Location(latitude, longitude);
        //}

    }
}
