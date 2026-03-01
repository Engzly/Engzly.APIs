using System.Net.Http.Headers;
using System.Net.Http.Json;
using Engzly.Application.Interfaces.Notifications;
using Microsoft.Extensions.Options;

namespace Engzly.Infrastructure.OtpSecurity.Notifications
{
    public class WhatsAppSender : IWhatsAppSender
    {
        private readonly HttpClient _http;


        public async Task SendAsync(string toPhoneE164, string message, CancellationToken ct)
        {
          
        }
    }
}

//EAANfPktlrVwBQ70X0Wg9F1oID216gWX8Phcz32U6oNt8Cgwsf6q0LwTFlKgZCHgZBbsG86yyyxmRrITReuwZBindFWlA1ypXFXwYraTbl7ZCPpsYReCZBVeGT3RZClKrZAazCqZAZB4Yh1dbsYncmiMHEyfhgPZBQZCURjZB3ogpUBeQXnZAHPh2Un1iRKQsSge819BWnQAZDZD