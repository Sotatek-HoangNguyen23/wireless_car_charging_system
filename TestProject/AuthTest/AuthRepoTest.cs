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

    }
}
