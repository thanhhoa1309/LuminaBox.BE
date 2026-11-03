using Microsoft.Extensions.Logging;
using LuminaBox.Application.Interface;

namespace LuminaBox.Infrastructure.Service
{
    // Development only: writes emails (including OTP codes) to the log instead of sending them
    public class LogEmailService : IEmailService
    {
        private readonly ILogger<LogEmailService> _logger;

        public LogEmailService(ILogger<LogEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            _logger.LogWarning("[EMAIL] To: {ToEmail} | Subject: {Subject} | Body: {HtmlBody}",
                toEmail, subject, htmlBody);
            return Task.CompletedTask;
        }
    }
}
