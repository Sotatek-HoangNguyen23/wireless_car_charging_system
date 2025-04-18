using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var server = smtpSettings["Server"];
            var port = smtpSettings["Port"];
            var senderEmail = Environment.GetEnvironmentVariable("SMTP_EMAIL")
                    ?? throw new InvalidOperationException("SMTP_EMAIL environment variable is missing");

            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                ?? throw new InvalidOperationException("SMTP_PASSWORD environment variable is missing");

            if (string.IsNullOrEmpty(server)){
                throw new InvalidOperationException("SMTP Server is required");
            }
            if (!int.TryParse(port, out var portNumber)) {
                throw new InvalidOperationException("Invalid SMTP port");
            }
            var senderName = smtpSettings["SenderName"]
                     ?? throw new ArgumentNullException("SenderName is required in SmtpSettings");
            var EmailMessage = new MimeMessage();
            EmailMessage.From.Add(new MailboxAddress(senderName, senderEmail));
            EmailMessage.To.Add(new MailboxAddress("",toEmail));
            EmailMessage.Subject = subject;
            EmailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };
            using var client = new SmtpClient();
            // Kết nối với SSL/TLS
            try
            {
                await client.ConnectAsync(server, portNumber, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, password); 
                await client.SendAsync(EmailMessage);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }

        }

    }
}
