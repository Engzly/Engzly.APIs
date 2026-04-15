namespace Engzly.Application.Responses.GigsResponse
{
    public sealed class StartGigResponse
    {
        public string GigId { get; set; } = null!;
        public string PaymentId { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal HelperAmount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string PaymentUrl { get; set; } = null!;
        public string ProviderInvoiceId { get; set; } = null!;
    }
}
