using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.UserTest
{
    [TestFixture]
    public class CccdRepositoryTests
    {
        private WccsContext _context;
        private CccdRepository _repository;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WccsContext(options);
            _repository = new CccdRepository(_context);

            var user = new User
            {
                UserId = 1,
                Email = "test@example.com",
                PhoneNumber = "0123456789",    
                Fullname = "Nguyen Van A",    
                CreateAt = DateTime.UtcNow,
            };
            await _context.Users.AddAsync(user);

            var cccd = new Cccd
            {
                CccdId = 1,
                Code = "123456789",
                UserId = 1,
                ImgFront = "front.jpg",
                ImgBack = "back.jpg",
                CreateAt = DateTime.UtcNow
            };
            await _context.Cccds.AddAsync(cccd);
            await _context.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        // GetCccdByCode Tests
        [Test]
        public async Task GetCccdByCode_ValidCode_ReturnsCccdWithUser()
        {
            // Act
            var result = await _repository.GetCccdByCode("123456789");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.Code, Is.EqualTo("123456789"));
                Assert.That(result.User.Email, Is.EqualTo("test@example.com"));
            });
        }

        [Test]
        public async Task GetCccdByCode_InvalidCode_ReturnsNull()
        {
            // Act
            var result = await _repository.GetCccdByCode("INVALID_CODE");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetCccdByCode_NullCode_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetCccdByCode(null!));
            Assert.That(ex.Message, Does.Contain("Code không thể trống"));
        }

        [Test]
        public void GetCccdByCode_WhiteSpaceCode_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetCccdByCode("   "));
            Assert.That(ex.Message, Does.Contain("Code không thể trống"));
        }

        // SaveCccd Tests
        [Test]
        public async Task SaveCccd_ValidCccd_SavesToDatabase()
        {
            // Arrange
            var newCccd = new Cccd
            {
                Code = "987654321",
                ImgFront = "new_front.jpg",
                ImgBack = "new_back.jpg",
                UserId = 1
            };

            // Act
            await _repository.SaveCccd(newCccd);

            // Assert
            var savedCccd = await _context.Cccds.FirstOrDefaultAsync(c => c.Code == "987654321");
            Assert.Multiple(() =>
            {
                Assert.That(savedCccd, Is.Not.Null);
                Assert.That(savedCccd!.ImgFront, Is.EqualTo("new_front.jpg"));
            });
        }

        [Test]
        public void SaveCccd_NullCccd_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.SaveCccd(null!));
            Assert.That(ex.Message, Does.Contain("Cccd không được null"));
        }

        [Test]
        public void SaveCccd_DuplicateCode_ThrowsDbUpdateException()
        {
            // Arrange
            var duplicateCccd = new Cccd
            {
                Code = "123456789", // Trùng code đã tồn tại
                ImgFront = "duplicate.jpg",
                UserId = 1
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                 _repository.SaveCccd(duplicateCccd));
            Assert.That(ex.Message, Does.Contain("Cccd đã tồn tại"));
        }
    }
}
