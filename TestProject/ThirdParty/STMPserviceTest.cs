using API.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using NUnit.Framework; // Add this import
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.ThirdParty
{
    [TestFixture]
    public class STMPserviceTest
    {
        private EmailService _emailService = null!;

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("SMTP_EMAIL", "dummy@example.com");
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", "dummy_password");
        }

        [TearDown]
        public void Cleanup()
        {
            // Dọn dẹp environment variables sau mỗi test
            Environment.SetEnvironmentVariable("SMTP_EMAIL", null);
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", null);
        }

        [Test]
        public void SendEmailAsync_MissingSmtpConfig_ThrowsException()
        {
            // Arrange

            var invalidConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"SmtpSettings:Server", ""},
                    {"SmtpSettings:Port", "587"}
                }).Build();

            _emailService = new EmailService(invalidConfig);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _emailService.SendEmailAsync("test@example.com", "Subject", "Content"));

            // Kiểm tra message về SMTP server
            Assert.That(ex.Message, Does.Contain("SMTP Server is required"));
        }

        [Test]
        public void SendEmailAsync_MissingEnvVariables_ThrowsAuthException()
        {
            Environment.SetEnvironmentVariable("SMTP_EMAIL", null);
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", null);
            // Arrange
            var validConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"SmtpSettings:Server", "smtp.example.com"},
                    {"SmtpSettings:Port", "587"},
                    {"SmtpSettings:SenderName", "Test Sender"}
                }).Build();

            _emailService = new EmailService(validConfig);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _emailService.SendEmailAsync("test@example.com", "Subject", "Content"));

            Assert.That(ex.Message, Does.Contain("SMTP_EMAIL environment variable is missing"));
        }

        [Test]
        public void SendEmailAsync_InvalidPort_ThrowsException()
        {
            // Arrange

            var invalidConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"SmtpSettings:Server", "smtp.example.com"},
                    {"SmtpSettings:Port", "invalid_port"}
                }).Build();

            _emailService = new EmailService(invalidConfig);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _emailService.SendEmailAsync("test@example.com", "Subject", "Content"));

            // Assert message về port
            Assert.That(ex.Message, Does.Contain("Invalid SMTP port"));
        }

        [Test]
        public void SendEmailAsync_InvalidCredentials_ThrowsAuthException()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SMTP_EMAIL", "wrong@example.com");
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", "wrong_password");

            var validConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"SmtpSettings:Server", "smtp.gmail.com"},
                    {"SmtpSettings:Port", "587"},
                    {"SmtpSettings:SenderName", "Test Sender"}
                }).Build();

            _emailService = new EmailService(validConfig);

            // Act & Assert
            Assert.ThrowsAsync<MailKit.Security.AuthenticationException>(() =>
                _emailService.SendEmailAsync("test@example.com", "Subject", "Content"));
        }

        [Test]
        public void SendEmailAsync_MissingSenderName_ThrowsException()
        {
            // Arrange

            var invalidConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"SmtpSettings:Server", "smtp.valid-dummy-server.com"},
                    {"SmtpSettings:Port", "587"},
                    // Thiếu SmtpSettings:SenderName
                }).Build();

            _emailService = new EmailService(invalidConfig);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(() =>
                _emailService.SendEmailAsync("test@example.com", "Subject", "Content"));

            Assert.That(ex.ParamName, Is.EqualTo("SenderName is required in SmtpSettings"));
        }
    }
}
