using Engzly.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Domain.Entities.Identity
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public string City { get; set; }
        public AccountType AccountType { get; private set; }
        public UserStatus Status { get;  set; } = UserStatus.Pending;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation Property to Document(Photos )



        public void Delete()
        {
            Status = UserStatus.Deleted;
            LockoutEnabled = true;
            LockoutEnd = DateTimeOffset.MaxValue;
        }
    }
}
