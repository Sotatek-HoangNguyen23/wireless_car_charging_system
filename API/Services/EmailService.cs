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
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(port))
            {
                throw new InvalidOperationException("SMTP settings are not configured properly.");
            }

            var EmailMessage = new MimeMessage();
            EmailMessage.From.Add(new MailboxAddress(smtpSettings["SenderName"], smtpSettings["SenderEmail"]));
            EmailMessage.To.Add(new MailboxAddress("",toEmail));
            EmailMessage.Subject = subject;
            EmailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };
            using var client = new SmtpClient();
            // Kết nối với SSL/TLS
            await client.ConnectAsync(server, int.Parse(port), SecureSocketOptions.StartTls);
            // Xác thực
            await client.AuthenticateAsync(
                smtpSettings["Username"],
                smtpSettings["Password"]
            );
            // Gửi email
            await client.SendAsync(EmailMessage);
            await client.DisconnectAsync(true);

        }

    }
}
