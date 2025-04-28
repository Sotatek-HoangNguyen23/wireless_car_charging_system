using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.MyCarTest
{
    [TestFixture]
    internal class URepoTest
    {
        private WccsContext _context;
        private UserRepository _repository;
        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WccsContext(options);
            _repository = new UserRepository(_context);

            // Set up base test data
            await SetupTestData();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        private async Task SetupTestData()
        {
            // Create roles
            var userRole = new Role
            {
                RoleId = 1,
                RoleName = "User"
            };

            var adminRole = new Role
            {
                RoleId = 2,
                RoleName = "Admin"
            };

            // Create users
            var user1 = new User
            {
                UserId = 1,
                RoleId = 1,
                Fullname = "Test User",
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                Dob = new DateTime(1990, 1, 1),
                Gender = true,
                Address = "123 Test Street",
                Status = "Active",
                CreateAt = DateTime.Now,
                Role = userRole
            };

            var user2 = new User
            {
                UserId = 2,
                RoleId = 1,
                Fullname = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "0987654321",
                Dob = new DateTime(1985, 5, 15),
                Gender = true,
                Address = "456 Sample Road",
                Status = "Active",
                CreateAt = DateTime.Now,
                Role = userRole
            };

            var user3 = new User
            {
                UserId = 3,
                RoleId = 2,
                Fullname = "Admin User",
                Email = "admin@example.com",
                PhoneNumber = "0123123123",
                Status = "Active",
                CreateAt = DateTime.Now,
                Role = adminRole
            };

            // Create CCCD for user1
            var cccd = new Cccd
            {
                CccdId = 1,
                UserId = 1,
                ImgFront = "front_image_url.jpg",
                ImgBack = "back_image_url.jpg",
                Code = "012345678901",
                CreateAt = DateTime.Now
            };

            // Add entities to context
            await _context.Roles.AddRangeAsync(userRole, adminRole);
            await _context.Users.AddRangeAsync(user1, user2, user3);
            await _context.Cccds.AddAsync(cccd);
            await _context.SaveChangesAsync();
        }
        // Tests for GetUserByEmailOrPhone
        [Test]
        public async Task GetUserByEmailOrPhone_WithEmail_ReturnsCorrectUsers()
        {
            // Act
            var result = await _repository.GetUserByEmailOrPhone("test@example.com");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Email, Is.EqualTo("test@example.com"));
            Assert.That(result[0].UserId, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUserByEmailOrPhone_WithPhoneNumber_ReturnsCorrectUsers()
        {
            // Act
            var result = await _repository.GetUserByEmailOrPhone("0123456789");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].PhoneNumber, Is.EqualTo("0123456789"));
            Assert.That(result[0].UserId, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUserByEmailOrPhone_WithPartialMatch_ReturnsAllMatchingUsers()
        {
            // Act - Search with partial email matching both user1 and user2
            var result = await _repository.GetUserByEmailOrPhone("example.com");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(3)); // Should match all three users
            Assert.That(result.Any(u => u.UserId == 1), Is.True);
            Assert.That(result.Any(u => u.UserId == 2), Is.True);
            Assert.That(result.Any(u => u.UserId == 3), Is.True);
        }

        [Test]
        public async Task GetUserByEmailOrPhone_WithPartialPhoneMatch_ReturnsMatchingUsers()
        {
            // Act
            var result = await _repository.GetUserByEmailOrPhone("123");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2)); // Should match user1 and user3
            Assert.That(result.Any(u => u.UserId == 1), Is.True);
            Assert.That(result.Any(u => u.UserId == 3), Is.True);
        }

        [Test]
        public async Task GetUserByEmailOrPhone_WithNoMatch_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetUserByEmailOrPhone("nonexistent");

            // Assert
            Assert.That(result, Is.Empty);
        }

        // Tests for GetProfileByUserId
        [Test]
        public async Task GetProfileByUserId_ValidUserId_ReturnsCorrectProfile()
        {
            // Act
            var result = await _repository.GetProfileByUserId(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.UserId, Is.EqualTo(1));
                Assert.That(result.Fullname, Is.EqualTo("Test User"));
                Assert.That(result.Email, Is.EqualTo("test@example.com"));
                Assert.That(result.PhoneNumber, Is.EqualTo("0123456789"));
                Assert.That(result.Dob, Is.EqualTo(new DateTime(1990, 1, 1)));
                Assert.That(result.Gender, Is.EqualTo(true));
                Assert.That(result.Address, Is.EqualTo("123 Test Street"));
                Assert.That(result.Status, Is.EqualTo("Active"));
                Assert.That(result.CccdId, Is.EqualTo(1));
                Assert.That(result.ImgFront, Is.EqualTo("front_image_url.jpg"));
                Assert.That(result.ImgBack, Is.EqualTo("back_image_url.jpg"));
                Assert.That(result.Code, Is.EqualTo("012345678901"));
            });
        }

        [Test]
        public async Task GetProfileByUserId_UserWithoutCccd_ReturnsProfileWithDefaultCccdValues()
        {
            // Act
            var result = await _repository.GetProfileByUserId(2);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.UserId, Is.EqualTo(2));
                Assert.That(result.Fullname, Is.EqualTo("John Doe"));
                Assert.That(result.Email, Is.EqualTo("john.doe@example.com"));
                Assert.That(result.PhoneNumber, Is.EqualTo("0987654321"));
                // CCCD related fields should be default values
                Assert.That(result.CccdId, Is.EqualTo(0)); // Default value for int
                Assert.That(result.ImgFront, Is.Null);
                Assert.That(result.ImgBack, Is.Null);
                Assert.That(result.Code, Is.Null);
            });
        }

        [Test]
        public async Task GetProfileByUserId_InvalidUserId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetProfileByUserId(999);

            // Assert
            Assert.That(result, Is.Null);
        }

    }
}
