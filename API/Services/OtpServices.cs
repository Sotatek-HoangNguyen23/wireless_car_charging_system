using DataAccess.DTOs.Auth;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    public interface IOtpServices
    {
        Task<string> GenerateOtpAsync(OtpRequest request);
        Task<bool> VerifyOtpAsync(string identifier, string inputOtp);
        Task<string> genResetPasswordToken(string email);
        Task<bool> verifyResetPasswordToken(string token, string email);
    }
    public class OtpServices : IOtpServices
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
            try
            {
                var otp = GenerateSecureOtp();
                var key = $"otp:{request.Email}";
                var db = _redis.GetDatabase();

                var otpData = new OtpData
                {
                    Code = HashToken(otp),
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
            catch (Exception e)
            {
                throw new Exception("Error generating OTP. Error:" + e);
            }
        }


        public async Task<bool> VerifyOtpAsync(string identifier, string inputOtp)
        {
            try
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

                if (otpData.Code != HashToken(inputOtp))
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
            catch (Exception e)
            {
                throw new Exception("Error verifying OTP. Error:"+e);
            }
        }
        public async Task<string> genResetPasswordToken(string email)
        {
            try
            {
                var token = GenerateSecureOtp();
                var key = $"reset-password-token:{token}";
                var db = _redis.GetDatabase();
                await db.StringSetAsync(key, email, TimeSpan.FromMinutes(OTP_EXPIRY_MINUTES));
                return token;
            }
            catch (Exception e)
            {
                throw new Exception("Error generating reset password token. Error:" + e);
            }
        }
        public async Task<bool> verifyResetPasswordToken(string token, string email)
        {
            try
            {
                var key = $"reset-password-token:{token}";
                var db = _redis.GetDatabase();
                var storedEmail = await db.StringGetAsync(key);
                if (storedEmail.IsNullOrEmpty)
                {
                    return false;
                }
                if (storedEmail.ToString() != email)
                {
                    return false;
                }
                await db.KeyDeleteAsync(key);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("Error verifying reset password token. Error:" + e);
            }
        }

        private string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashBytes);
            }

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
