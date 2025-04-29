using NUnit.Framework;
using StackExchange.Redis;
using API.Services;
using DataAccess.DTOs.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Security.Cryptography;

namespace TestProject.ThirdParty
{
    [TestFixture]
    public class OtpServicesRealTests
    {
        //Intergration test với Redis
        private IConnectionMultiplexer _redis;
        private OtpServices _otpServices;
        private const string TestEmail = "test@example.com";
        private IDatabase _redisDb;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Load configuration từ appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            Environment.SetEnvironmentVariable("REDIS_PASSWORD", "pcN7K4vO8TgSP4Tkwyyu4utvD16ghkzP");
            var configValues = new Dictionary<string, string>
            {
                {"Redis:ConnectionString", "redis-16850.c282.east-us-mz.azure.redns.redis-cloud.com:16850"}
            };
            var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

            var redisConnectionString = config["Redis:ConnectionString"];
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured.");
            }
            var configurationOptions = ConfigurationOptions.Parse(redisConnectionString); configurationOptions.Password = redisPassword;
            configurationOptions.AbortOnConnectFail = false;
            configurationOptions.ConnectTimeout = 15000;

            _redis = ConnectionMultiplexer.Connect(configurationOptions);
            _otpServices = new OtpServices(_redis);
            _redisDb = _redis.GetDatabase();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            if (_redis != null)
            {
                await _redis.CloseAsync();
                await _redis.DisposeAsync();
            }
        }

        [TearDown]
        public async Task Cleanup()
        {
            // Xóa dữ liệu test sau mỗi test case
            await _redisDb.KeyDeleteAsync($"otp:{TestEmail}");
            await _redisDb.KeyDeleteAsync($"reset-password-token:123456");
        }

        [Test]
        public async Task GenerateOtpAsync_ShouldStoreDataInRedis()
        {
            // Arrange
            var request = new OtpRequest { Email = TestEmail };

            // Act
            var otp = await _otpServices.GenerateOtpAsync(request);

            // Assert
            var data = await _redisDb.HashGetAllAsync($"otp:{TestEmail}");

            Assert.Multiple(() =>
            {
                Assert.That(data, Has.Length.EqualTo(3));
                Assert.That(data.FirstOrDefault(x => x.Name == "Code").Value.ToString(), Is.Not.Empty);
                Assert.That(data.FirstOrDefault(x => x.Name == "Attempts").Value.ToString(), Is.EqualTo("0"));
            });
        }

        [Test]
        public async Task VerifyOtpAsync_WithValidOtp_ShouldReturnTrue()
        {
            // Arrange
            var request = new OtpRequest { Email = TestEmail };
            var otp = await _otpServices.GenerateOtpAsync(request);

            // Act
            var result = await _otpServices.VerifyOtpAsync(TestEmail, otp);

            // Assert
            Assert.That(result, Is.True);
            var keyExists = await _redisDb.KeyExistsAsync($"otp:{TestEmail}");
            Assert.That(keyExists, Is.False);
        }

        [Test]
        public async Task VerifyOtpAsync_WithInvalidOtp_ShouldIncrementAttempts()
        {
            // Arrange
            var request = new OtpRequest { Email = TestEmail };
            await _otpServices.GenerateOtpAsync(request);

            // Act
            await _otpServices.VerifyOtpAsync(TestEmail, "wrong_otp"); // Lần 1
            await _otpServices.VerifyOtpAsync(TestEmail, "wrong_otp"); // Lần 2

            // Assert
            var attempts = await _redisDb.HashGetAsync($"otp:{TestEmail}", "Attempts");
            Assert.That(attempts.ToString(), Is.EqualTo("2"));
        }

        [Test]
        public async Task VerifyResetPasswordToken_ValidToken_ShouldWork()
        {
            // Arrange
            var token = await _otpServices.genResetPasswordToken(TestEmail);

            // Act
            var result = await _otpServices.verifyResetPasswordToken(token, TestEmail);

            // Assert
            Assert.That(result, Is.True);
            var storedValue = await _redisDb.StringGetAsync($"reset-password-token:{token}");
            Assert.That(storedValue.IsNullOrEmpty, Is.True);
        }
    }

    // Helper để hash OTP như trong class OtpServices
    public static class HashHelper
    {
        public static string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
