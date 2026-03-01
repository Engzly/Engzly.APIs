using System.Net;
using System.Net.Mail;
using Engzly.Application.Interfaces.Notifications;
using Microsoft.Extensions.Options;

namespace Engzly.Infrastructure.OtpSecurity
{
  public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        public SmtpEmailSender(IOptions<SmtpOptions> opt) => _opt = opt.Value;

        public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
        {
            using var client = new SmtpClient(_opt.Host, _opt.Port)
            {
                EnableSsl = _opt.EnableSsl,
                Credentials = new NetworkCredential(_opt.Username, _opt.Password)
            };

            var mail = new MailMessage(_opt.From, toEmail, subject, body);
            await client.SendMailAsync(mail, ct);
        }
    }

    public class SmtpOptions
    {
        public string Host { get; set; }
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string From { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    



}