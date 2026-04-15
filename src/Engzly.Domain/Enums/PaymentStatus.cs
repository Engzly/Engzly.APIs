namespace Engzly.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,
        AwaitingFunding = 2,
        Funded = 3,
        Released = 4,
        Refunded = 5,
        Failed = 6,
        Expired = 7
    }
}
