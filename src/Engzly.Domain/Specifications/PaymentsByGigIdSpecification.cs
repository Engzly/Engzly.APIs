using Engzly.Domain.Entities.Payments;

namespace Engzly.Domain.Specifications
{
    public class PaymentsByGigIdSpecification : BaseSpecification<Payment>
    {
        public PaymentsByGigIdSpecification(string gigId)
        : base(p => p.GigId == gigId)
        {
            AddInclude(p => p.Events);
        }
    }
}
