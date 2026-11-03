using Microsoft.Extensions.Configuration;
using LuminaBox.Application.Interface;
using Resend;

namespace LuminaBox.Infrastructure.Service
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly string _fromEmail;

        public ResendEmailService(IResend resend, IConfiguration configuration)
        {
            _resend = resend;
            _fromEmail = configuration["Resend:FromEmail"]
                ?? throw new InvalidOperationException("Resend:FromEmail not found in configuration.");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new EmailMessage
            {
                From = _fromEmail,
                Subject = subject,
                HtmlBody = htmlBody
            };
            message.To.Add(toEmail);

            await _resend.EmailSendAsync(message);
        }
    }
}
