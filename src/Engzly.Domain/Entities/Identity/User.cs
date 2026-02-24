using Engzly.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Domain.Entities.Identity
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public Location Location { get; set; }
        public AccountType AccountType { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Pending;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

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
