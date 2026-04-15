using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Identity;

namespace Engzly.Domain.Entities.Payments
{
    public sealed class HelperWallet : BaseEntity<string>
    {
        public string UserId { get; set; } = null!;
        public string Currency { get; set; } = "EGP";
        public decimal Balance { get; set; } = 0m;
        public decimal PendingBalance { get; set; } = 0m;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
