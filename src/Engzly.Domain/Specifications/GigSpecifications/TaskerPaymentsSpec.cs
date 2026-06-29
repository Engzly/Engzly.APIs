using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;

namespace Engzly.Domain.Specifications.GigSpecifications
{
    public class TaskerPaymentsSpec
    : BaseSpecification<Payment>
    {
        public TaskerPaymentsSpec(string userId)
            : base(p =>
                p.HelperUserId == userId &&
                p.Status == PaymentStatus.Released)
        {
        }
    }
}
