namespace Engzly.Application.Interfaces.Notifications
{
    public interface IWhatsAppSender
    {
        Task SendAsync(string toPhoneE164, string message, CancellationToken ct);
    }
    
}