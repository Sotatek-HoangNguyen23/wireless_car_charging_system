using DataAccess.DTOs.Auth;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    public class OtpServices
    {
        private readonly IConnectionMultiplexer _redis;
        private const int OTP_EXPIRY_MINUTES = 5;
        private const int MAX_ATTEMPTS = 3;

        public OtpServices(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<string> GenerateOtpAsync(OtpRequest request)
        {

            var otp = GenerateSecureOtp();
            var key = $"otp:{request.Email}";
            var db = _redis.GetDatabase();

            var otpData = new OtpData
            {
                Code = HashOtp(otp),
                Created = DateTime.UtcNow,
                Attempts = 0
            };
            db.HashSet(key, new HashEntry[] {
                new HashEntry("Code", otpData.Code),
                new HashEntry("Created", otpData.Created.ToString("o")),
                new HashEntry("Attempts", otpData.Attempts)
            });
            await db.KeyExpireAsync(key, TimeSpan.FromMinutes(OTP_EXPIRY_MINUTES));
            return otp;
        }

        private string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(otp);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<bool> VerifyOtpAsync(string identifier, string inputOtp)
        {
            var key = $"otp:{identifier}";
            var db = _redis.GetDatabase();
            var data = await db.HashGetAllAsync(key);
            if (data.Length == 0)
            {
                return false;
            }
            var codeEntry = data.FirstOrDefault(x => x.Name == "Code");
            var createdEntry = data.FirstOrDefault(x => x.Name == "Created");
            var attemptsEntry = data.FirstOrDefault(x => x.Name == "Attempts");

            if (codeEntry.Value.IsNullOrEmpty || createdEntry.Value.IsNullOrEmpty || attemptsEntry.Value.IsNullOrEmpty)
            {
                return false;
            }

            var otpData = new OtpData
            {
                Code = codeEntry.Value.ToString(),
                Created = DateTime.Parse(createdEntry.Value.ToString()),
                Attempts = int.Parse(attemptsEntry.Value.ToString())
            };

            if (otpData.Attempts >= MAX_ATTEMPTS)
            {
                return false;
            }

            if (otpData.Code != HashOtp(inputOtp))
            {
                otpData.Attempts++;
                await db.HashSetAsync(key, new HashEntry[] {
                    new HashEntry("Attempts", otpData.Attempts)
                });
                return false;
            }

            await db.KeyDeleteAsync(key);
            return true;
        }

        private string GenerateSecureOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var number = BitConverter.ToUInt32(bytes) % 1000000;
            return number.ToString("D6"); 
        }


        private class OtpData
        {
            public required string Code { get; set; }
            public DateTime Created { get; set; }
            public int Attempts { get; set; }
        }
    }

}
