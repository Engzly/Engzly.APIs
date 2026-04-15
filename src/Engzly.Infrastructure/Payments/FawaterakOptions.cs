namespace Engzly.Infrastructure.Payments
{
    public sealed class FawaterakOptions
    {
        public const string SectionName = "Fawaterak";

        public string BaseUrl { get; set; } = "https://staging.fawaterk.com";
        public string ApiKey { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
        public string HashKey { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string FailUrl { get; set; } = string.Empty;
        public string PendingUrl { get; set; } = string.Empty;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public decimal PlatformCommissionRate { get; set; } = 0.05m;
    }
}
