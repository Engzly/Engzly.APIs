namespace Engzly.Domain.Enums
{
    public enum PaymentEventType
    {
        Created = 1,
        InvoiceCreated = 2,
        WebhookReceived = 3,
        Funded = 4,
        Released = 5,
        Refunded = 6,
        Failed = 7,
        ReconciliationChecked = 8
    }
}
