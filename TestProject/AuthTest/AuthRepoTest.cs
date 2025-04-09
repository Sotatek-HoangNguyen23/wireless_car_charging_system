using DataAccess.Models;
using DataAccess.Repositories;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.AuthTest
{
    [TestFixture]
    public class AuthRepoTest
    {
        private AuthRepository _repository;
        private WccsContext _context;
        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
            _context = new WccsContext(options);
            _repository = new AuthRepository(_context);
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
                RoleId = 1,
                Email = "test@example.com", 
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
        public async Task FindRefreshToken_ExistingToken_ReturnsToken()
        {
            // Arrange
            var testToken = new RefreshToken
            {
                Token = "valid_token_123",
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(testToken);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindRefreshToken("valid_token_123");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.EqualTo("valid_token_123"));
            Assert.That(result.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(7)).Within(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public async Task FindRefreshToken_NonExistentToken_ReturnsNull()
        {
            var result = await _repository.FindRefreshToken("invalid_token_999");

            Assert.That(result, Is.Null);
        }
        [Test]
        public void FindRefreshToken_EmptyToken_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.FindRefreshToken(""));
            Assert.That(ex.Message, Does.Contain("Token không thể trống"));
        }
        [Test]
        public async Task FindRefreshToken_ExistentToken_ReturnsNotNull()
        {
            var testToken = new RefreshToken
            {
                Token = "valid_token_123",
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(testToken);
            await _context.SaveChangesAsync();
            var result = await _repository.FindRefreshToken("valid_token_123");
            Assert.That(result, Is.Not.Null);
        }
        [Test]
        public async Task FindRefreshToken_IncludesUserNavigation()
        {
            // Arrange
            var testToken = new RefreshToken
            {
                Token = "with_user_token",
                UserId = 1,
                User = await _context.Users.FirstAsync()
            };
            _context.RefreshTokens.Add(testToken);
            await _context.SaveChangesAsync();

            // Act
            var result = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == "with_user_token");

            // Assert
            Assert.That(result!.User, Is.Not.Null);
            Assert.That(result.User.Email, Is.EqualTo("test@example.com"));
        }
        [Test]
        public async Task SaveRefreshToken_PersistsAllFieldsCorrectly()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();
            const string expectedToken = "full_fields_token";

            // Act
            await _repository.SaveRefreshToken(expectedToken, user);

            // Assert
            var savedToken = await _context.RefreshTokens.FirstAsync();

            Assert.Multiple(() =>
            {
                Assert.That(savedToken.Token, Is.EqualTo(expectedToken));
                Assert.That(savedToken.UserId, Is.EqualTo(1));
                Assert.That(savedToken.Revoked, Is.False);
                Assert.That(savedToken.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(1).Seconds);
                Assert.That(savedToken.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(7)).Within(1).Seconds);
            });
        }
        [Test]
        public async Task SaveRefreshToken_ValidInput_SavesTokenWithCorrectData()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();
            const string token = "new_token_123";

            // Act
            await _repository.SaveRefreshToken(token, user);

            // Assert
            var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync();
            Assert.That(savedToken, Is.Not.Null);
            Assert.That(savedToken.Token, Is.EqualTo(token));
            Assert.That(savedToken.UserId, Is.EqualTo(user.UserId));
            Assert.That(savedToken.Revoked, Is.False);
            Assert.That(savedToken.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(7)).Within(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void SaveRefreshToken_NullUser_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.SaveRefreshToken("token", null!));
            Assert.That(ex.ParamName, Is.EqualTo("user"));
        }
        [Test]
        public async Task UpdateRefreshTokenAsync_UpdatesRevokedStatus()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "revokable_token",
                UserId = 1,
                Revoked = false
            };
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();

            // Act
            token.Revoked = true;
            await _repository.UpdateRefreshTokenAsync(token);

            // Assert
            var updatedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == "revokable_token");
            Assert.That(updatedToken?.Revoked, Is.True);
        }
        [Test]
        public async Task UpdateRefreshTokenAsync_NonExistentToken_ThrowsException()
        {
            // Arrange
            var nonExistentToken = new RefreshToken { Token = "ghost_token" };

            // Act & Assert
            await Task.Run(() => Assert.ThrowsAsync<InvalidOperationException>(() =>
                _repository.UpdateRefreshTokenAsync(nonExistentToken)));
        }

        [Test]
        public async Task SaveRefreshToken_InvalidUserId_ThrowsInvalidOperationException()
        {
            // Arrange: Tạo đối tượng User với UserId không tồn tại trong DB
            var invalidUser = new User { UserId = 999 };

            // Act & Assert
            var ex = await Task.Run(() => Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _repository.SaveRefreshToken("invalid_token", invalidUser);
            }));
            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public async Task SaveRefreshToken_MultipleTokensSameUser_SavesSuccessfully()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();

            // Act
            await _repository.SaveRefreshToken("token1", user);
            await _repository.SaveRefreshToken("token2", user);

            // Assert
            var tokens = await _context.RefreshTokens.CountAsync();
            Assert.That(tokens, Is.EqualTo(2));
        }
        [Test]
        public async Task SaveRefreshToken_EnsuresUtcTime()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();

            // Act
            await _repository.SaveRefreshToken("utc_test", user);

            // Assert
            var savedToken = await _context.RefreshTokens.FirstAsync();
            Assert.That(savedToken.CreatedAt!.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(savedToken.ExpiresAt!.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
        }
      
    }
}
