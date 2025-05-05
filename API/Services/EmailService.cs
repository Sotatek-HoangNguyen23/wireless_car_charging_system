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
                    ?? throw new InvalidOperationException("SMTP_EMAIL biến môi trường bị thiếu");

            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                ?? throw new InvalidOperationException("SMTP_PASSWORD biến môi trường bị thiếu");

            if (string.IsNullOrEmpty(server)){
                throw new InvalidOperationException("SMTP Server bị thiếu");
            }
            if (!int.TryParse(port, out var portNumber)) {
                throw new InvalidOperationException("SMTP port không hợp lên");
            }
            var senderName = smtpSettings["SenderName"]
                     ?? throw new ArgumentNullException("SenderName bị thiếu trong SmtpSettings");
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
