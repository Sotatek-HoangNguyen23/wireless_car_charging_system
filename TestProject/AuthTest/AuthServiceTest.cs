using API.Services;
using DataAccess.DTOs.Auth;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.AuthTest
{
    public class AuthServiceTest
    {
        private AuthService _authService;
        private WccsContext _context;
        private IAuthRepository _authRepository;
        private IUserRepository _userRepository;
        private IConfiguration _configuration;
        [SetUp]
        public async Task Setup()
        {
            Environment.SetEnvironmentVariable("JWT_SECRET", "TestSecretKey123!@#TestSecretKey456!@#");
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            // Khởi tạo In-Memory Database
            _context = new WccsContext(options);
            // Khởi tạo repository với In-Memory Database
            _authRepository = new AuthRepository(_context);
            _userRepository = new UserRepository(_context);
            var configValues = new Dictionary<string, string>
            {
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
            }; 
            _configuration = new ConfigurationBuilder()
                 .AddEnvironmentVariables()
                .AddInMemoryCollection(configValues!)
                .Build();
            _authService = new AuthService(_authRepository, _userRepository, _configuration);

            await SeedTestData();

        }
        private async Task SeedTestData()
        {
            // Thêm role
            var roles = new List<Role>
            {
                new Role { RoleId = 1, RoleName = "Driver" },
                new Role { RoleId = 2, RoleName = "Station owner" },
                new Role { RoleId = 3, RoleName = "Operator" },
                new Role { RoleId = 4, RoleName = "Admin" }
            };
            var user = new User
            {
                UserId = 1,
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123"),
                RoleId = 1, 
                Fullname = "Test User", 
                PhoneNumber = "0123456789",
                CreateAt = DateTime.UtcNow
            };
            await _context.Roles.AddRangeAsync(roles);
            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();
        }
        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
        [Test]
        public async Task Authenticate_ValidCredentials_ReturnsResponseWithTokens()
        {
            // Arrange
            var request = new AuthenticateRequest
            {
                Email = "test@example.com",
                Password = "TestPassword123"
            };

            // Act
            var result = await _authService.Authenticate(request);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.AccessToken, Is.Not.Null.And.Not.Empty);
                Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
                Assert.That(result.Fullname, Is.EqualTo("Test User"));
                Assert.That(result.Role, Is.EqualTo("Driver"));
            });

            // Verify refresh token saved
            var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync();
            Assert.That(savedToken, Is.Not.Null);
            Assert.That(savedToken.UserId, Is.EqualTo(1));
        }
        [Test]
        public void Authenticate_InvalidEmail_ReturnsNull()
        {
            // Arrange
            var request = new AuthenticateRequest
            {
                Email = "wrong@example.com",
                Password = "TestPassword123"
            };

            // Act
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _authService.Authenticate(request));

            // Assert exception message
            Assert.That(ex.Message, Does.Contain("Tai khoan khong ton tai"));
        }
        [Test]
        public void Authenticate_InvalidPassword_ThrowException()
        {
            // Arrange
            var request = new AuthenticateRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword" 
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _authService.Authenticate(request));

            // Assert exception message
            Assert.That(ex.Message, Does.Contain("Mat khau sai"));
        }

        [Test]
        public void Authenticate_NullEmail_ThrowsException()
        {
            // Arrange
            var request = new AuthenticateRequest
            {
                Email = null!,
                Password = "TestPassword123"
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => _authService.Authenticate(request));
        }

        [Test]
        public void Authenticate_NullPassword_ThrowsException()
        {
            // Arrange
            var request = new AuthenticateRequest
            {
                Email = "test@example.com",
                Password = null!
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => _authService.Authenticate(request));
        }
        [Test]
        public void Authenticate_EmptyEmail_ThrowsException()
        {
            // Arrange
            var request = new AuthenticateRequest
            {
                Email = "",
                Password = "TestPassword123"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _authService.Authenticate(request));

            Assert.Multiple(() =>
            {
                Assert.That(ex.ParamName, Is.EqualTo("email"));
                Assert.That(ex.Message, Does.Contain("không thể trống"));
            });
        }

        [Test]
        public void Authenticate_EmptyPassword_ThrowsException()
        {
            // Arrange
            var request = new AuthenticateRequest
            {
                Email = "test@example.com",
                Password = ""
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _authService.Authenticate(request));

            Assert.Multiple(() =>
            {
                Assert.That(ex.ParamName, Is.EqualTo("Password"));
                Assert.That(ex.Message, Does.Contain("không thể trống"));
            });
        }
        [Test]
        public void Authenticate_EmailExceedsMaxLength_ThrowsException()
        {
            // Arrange: Tạo email dài 257 ký tự
            var longEmail = new string('a', 226);
            var request = new AuthenticateRequest { Email = longEmail, Password = "ValidPassword123" };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _authService.Authenticate(request));
            Assert.That(ex.ParamName, Is.EqualTo("Email"));
            Assert.That(ex.Message, Does.Contain("225 ký tự"));
        }

        [Test]
        public void Authenticate_PasswordExceedsMaxLength_ThrowsException()
        {
            // Arrange: Tạo password dài 101 ký tự
            var longPassword = new string('a', 101);
            var request = new AuthenticateRequest { Email = "valid@example.com", Password = longPassword };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _authService.Authenticate(request));
            Assert.That(ex.ParamName, Is.EqualTo("Password"));
            Assert.That(ex.Message, Does.Contain("100 ký tự"));
        }

        [Test]
        public async Task SaveRefreshToken_ShouldSaveToDatabase()
        {
            // Arrange
            var user = await _userRepository.GetUserByEmail("test@example.com");
            Assert.That(user, Is.Not.Null, "User không tồn tại trong database"); 

            var token = "test_token";

            // Act
            await _authRepository.SaveRefreshToken(token, user);

            // Assert
            var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync();
            Assert.That(savedToken, Is.Not.Null);
            Assert.That(savedToken.Token, Is.EqualTo(token));
        }
        [Test]
        public void RefreshToken_EmptyToken_ThrowsException()
        {
            // Arrange
            var emptyToken = "";

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _authService.RefreshToken(emptyToken));
            Assert.That(ex.ParamName, Is.EqualTo("oldRefreshToken"));
            Assert.That(ex.Message, Does.Contain("không thể trống"));
        }

        [Test]
        public void RefreshToken_InvalidToken_ThrowsException()
        {
            // Arrange
            var invalidToken = "invalid_token";

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _authService.RefreshToken(invalidToken));
            Assert.That(ex.Message, Does.Contain("Invalid refresh token"));
        }
        [Test]
        public async Task RefreshToken_ValidToken_ReturnsNewTokensAndRevokesOld()
        {
            // Arrange 
            var user = await _userRepository.GetUserByEmail("test@example.com");
            var oldRefreshToken = "valid_refresh_token_123";
            var hashedOldToken = HashToken(oldRefreshToken);

            // Thêm token vào database
            await _authRepository.SaveRefreshToken(hashedOldToken, user!);

            // Act 
            var result = await _authService.RefreshToken(oldRefreshToken);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
                Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);
                Assert.That(result.RefreshToken, Is.Not.EqualTo(oldRefreshToken));
            });

            // Kiểm tra token cũ bị revoked
            var revokedToken = await _authRepository.FindRefreshToken(hashedOldToken);
            Assert.That(revokedToken!.Revoked, Is.True);

            // Kiểm tra token mới được lưu
            var newTokenHash = HashToken(result.RefreshToken);
            var newToken = await _authRepository.FindRefreshToken(newTokenHash);
            Assert.Multiple(() =>
            {
                Assert.That(newToken, Is.Not.Null);
                Assert.That(newToken!.Revoked, Is.False);
                Assert.That(newToken.ExpiresAt, Is.GreaterThan(DateTime.UtcNow));
            });

            // Giải mã access token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(result.AccessToken);

            // Kiểm tra claims
            Assert.Multiple(() =>
            {
                Assert.That(jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value,
                    Is.EqualTo("test@example.com"));
                Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value,
                    Is.EqualTo("Driver"));
                Assert.That(jwtToken.Issuer, Is.EqualTo("TestIssuer"));
                Assert.That(jwtToken.Audiences.First(), Is.EqualTo("TestAudience"));
            });
        }
        private string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashBytes);
            }
        }
        [Test]
        public async Task RefreshToken_ExpiredToken_ThrowsException()
        {
            // Arrange
            var user = await _userRepository.GetUserByEmail("test@example.com");
            var oldRefreshToken = "expired_token";
            var hashedToken = HashToken(oldRefreshToken);
            var expiredToken = new RefreshToken
            {
                Token = hashedToken,
                UserId = user!.UserId,
                ExpiresAt = DateTime.UtcNow.AddHours(-1),
                Revoked = false
            };
            await _context.RefreshTokens.AddAsync(expiredToken);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _authService.RefreshToken(oldRefreshToken));
            Assert.That(ex.Message, Does.Contain("da het han"));
        }

        [Test]
        public async Task RefreshToken_RevokedToken_ThrowsException()
        {
            // Arrange
            var user = await _userRepository.GetUserByEmail("test@example.com");
            var oldRefreshToken = "revoked_token";
            var hashedToken = HashToken(oldRefreshToken);
            var revokedToken = new RefreshToken
            {
                Token = hashedToken,
                UserId = user!.UserId,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Revoked = true
            };
            await _context.RefreshTokens.AddAsync(revokedToken);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _authService.RefreshToken(oldRefreshToken));
            Assert.That(ex.Message, Does.Contain("da bi thu hoi"));
        }
        [Test]
        public async Task RefreshToken_UserNotFound_ThrowsException()
        {
            // Arrange
            var invalidUserId = 999; // Non-existent user
            var oldRefreshToken = "valid_token_for_nonexistent_user";
            var hashedToken = HashToken(oldRefreshToken);
            var tokenWithInvalidUser = new RefreshToken
            {
                Token = hashedToken,
                UserId = invalidUserId,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Revoked = false
            };
            await _context.RefreshTokens.AddAsync(tokenWithInvalidUser);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _authService.RefreshToken(oldRefreshToken));
            Assert.That(ex.Message, Does.Contain("khong ton tai"));
        }
        [Test]
        public async Task RefreshToken_ReturnsNewRefreshToken()
        {
            // Arrange
            var user = await _userRepository.GetUserByEmail("test@example.com");
            var oldToken = "old_valid_token";
            await _authRepository.SaveRefreshToken(HashToken(oldToken), user!);

            // Act
            var result = await _authService.RefreshToken(oldToken);

            // Assert
            Assert.That(result.RefreshToken, Is.Not.EqualTo(oldToken));
        }
        [Test]
        public async Task GetUserByEmail_ValidEmail_ReturnsUserWithRole()
        {
            // Act
            var user = await _userRepository.GetUserByEmail("test@example.com");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(user, Is.Not.Null);
                Assert.That(user!.Email, Is.EqualTo("test@example.com"));
                Assert.That(user.Role.RoleName, Is.EqualTo("Driver"));
            });
        }

        [Test]
        public async Task GetUserByEmail_InvalidEmail_ReturnsNull()
        {
            // Act
            var user = await _userRepository.GetUserByEmail("invalid@test.com");

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public void GetUserByEmail_EmptyEmail_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _userRepository.GetUserByEmail(""));
            Assert.That(ex.Message, Does.Contain("Email không thể"));
        }
        [Test]
        public async Task Logout_ValidToken_RevokesToken()
        {
            // Arrange
            var user = await _userRepository.GetUserByEmail("test@example.com");
            var refreshToken = "valid_logout_token";
            var hashedToken = HashToken(refreshToken);
            await _authRepository.SaveRefreshToken(hashedToken, user!);

            // Act
            await _authService.Logout(refreshToken);

            // Assert
            var tokenInDb = await _authRepository.FindRefreshToken(hashedToken);
            Assert.That(tokenInDb!.Revoked, Is.True);
        }
        [Test]
        public async Task Logout_InvalidToken_DoesNotThrow()
        {
            // Arrange
            var invalidToken = "non_existent_token";

            // Act & Assert
            await Task.Run(() => Assert.DoesNotThrowAsync(async () => await _authService.Logout(invalidToken)));
        }

    }
}
