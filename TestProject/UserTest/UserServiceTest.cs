using API.Services;
using CloudinaryDotNet.Actions;
using DataAccess.DTOs.Auth;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace TestProject.UserTest
{
    [TestFixture]
    public class UserServiceTest
    {
        private WccsContext _context;
        private IUserRepository _userRepository;
        private ICccdRepository _cccdRepository;
        private IDriverLicenseRepository _licenseRepository;
        private IBalancement _balanceRepository;
        private ImageService _imageService;
        private OtpServices _otpServices;
        private UserService _userService;

        private Mock<ICloudinaryService> _cloudinaryServiceMock;
        private Mock<IConnectionMultiplexer> _redisMock;
        private RegisterRequest _validRequest;
        private Mock<IFormFile> _mockFrontImage;
        private Mock<IFormFile> _mockBackImage;
        [SetUp]
        public Task Setup()
        {
            // Cấu hình In-Memory Database
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new WccsContext(options);

            // Khởi tạo repositories
            _userRepository = new UserRepository(_context);
            _cccdRepository = new CccdRepository(_context);
            _licenseRepository = new DriverLicenseRepository(_context);
            _balanceRepository = new BalanceRepo(_context);

            // Mock Redis
            _redisMock = new Mock<IConnectionMultiplexer>();
            var dbMock = new Mock<IDatabase>();
            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                     .Returns(dbMock.Object);
            _otpServices = new OtpServices(_redisMock.Object);

            // Mock Cloudinary
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();
            _imageService = new ImageService(_cloudinaryServiceMock.Object);

            // Khởi tạo UserService với các dependencies
            _userService = new UserService(
                _userRepository,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );
            _mockFrontImage = new Mock<IFormFile>();
            _mockFrontImage.Setup(f => f.FileName).Returns("front.jpg");
            _mockFrontImage.Setup(f => f.Length).Returns(1024);

            _mockBackImage = new Mock<IFormFile>();
            _mockBackImage.Setup(f => f.FileName).Returns("back.jpg");
            _mockBackImage.Setup(f => f.Length).Returns(1024);
            var roles = new List<DataAccess.Models.Role>
            {
                new DataAccess.Models.Role { RoleId = 1, RoleName = "Driver" },
                new DataAccess.Models.Role { RoleId = 2, RoleName = "Station owner" },
                new DataAccess.Models.Role { RoleId = 3, RoleName = "Operator" },
                new DataAccess.Models.Role { RoleId = 4, RoleName = "Admin" }
            };
            _context.Roles.AddRange(roles);
            _validRequest = new RegisterRequest
            {
                Email = "test@example.com",
                PasswordHash = "P@ssw0rd123!",
                Fullname = "Test User",
                PhoneNumber = "0987654321",
                Dob = new DateTime(1990, 1, 1),
                Gender = true,
                Address = "Hanoi",
                CccdCode = "123456789",
                CCCDFrontImage = _mockFrontImage.Object,
                CCCDBackImage = _mockBackImage.Object
            };
            return Task.CompletedTask;
        }
        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
        [Test]
        public void RegisterAsync_ShouldThrowExceptionWhenRequestIsNull()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.RegisterAsync(null!));
            Assert.That(ex.Message, Does.Contain("Request không được null"));
        }

        [Test]
        public async Task RegisterAsync_ShouldThrowExceptionWhenEmailExists()
        {
            // Arrange
            await _userRepository.SaveUser(new User { Email = _validRequest.Email });

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _userService.RegisterAsync(_validRequest));
            Assert.That(ex.Message, Does.Contain("Email đã tồn tại"));
        }

        [Test]
        public async Task RegisterAsync_ShouldThrowExceptionWhenCccdExists()
        {
            // Arrange
            await _cccdRepository.SaveCccd(new Cccd { Code = _validRequest.CccdCode });

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _userService.RegisterAsync(_validRequest));
            Assert.That(ex.Message, Does.Contain("CCCD đã tồn tại"));
        }

        [Test]
        public async Task RegisterAsync_ShouldThrowExceptionWhenPhoneExists()
        {
            // Arrange
            var existingUser = new User
            {
                PhoneNumber = _validRequest.PhoneNumber,
                Email = "dummy@test.com",
                PasswordHash = "dummy_hash",
                Fullname = "Dummy User",
                Dob = DateTime.UtcNow,
                Gender = true,
                Address = "Dummy Address",
                RoleId = 1, // Matches the Role added above
                Status = "Active",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            await _userRepository.SaveUser(existingUser);

            // Verify existing user is saved
            var savedUser = await _userRepository.GetUserByPhone(_validRequest.PhoneNumber);
            Assert.That(savedUser, Is.Not.Null, "Existing user with phone number not found.");

            // Mock image upload to prevent NullReferenceException
            _cloudinaryServiceMock
                .Setup(c => c.UploadAsync(It.IsAny<ImageUploadParams>()))
                .ReturnsAsync(new ImageUploadResult
                {
                    Url = new Uri("http://mock-url.com"),
                    PublicId = "public_id"
                });

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.RegisterAsync(_validRequest));

            Assert.That(ex.Message, Does.Contain("Số điện thoại đã tồn tại"));
        }
        [Test]
        public void RegisterAsync_ShouldThrowExceptionWhenPasswordIsWeak()
        {
            // Arrange
            _validRequest.PasswordHash = "weak";

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.RegisterAsync(_validRequest));
            Assert.That(ex.Message, Does.Contain("Mật khẩu không đủ mạnh"));
        }

        [Test]
        public async Task RegisterAsync_ShouldCreateUserAndCccdWhenValidRequest()
        {
            // Arrange
            _cloudinaryServiceMock
                .Setup(c => c.UploadAsync(It.IsAny<ImageUploadParams>()))
                .ReturnsAsync(new ImageUploadResult
                {
                    Url = new Uri("http://mock-url.com"),
                    PublicId = "public_id"
                });

            // Act
            await _userService.RegisterAsync(_validRequest);

            // Assert
            var user = await _userRepository.GetUserByEmail(_validRequest.Email);
            Assert.That(user,Is.Not.Null);
            Assert.That(user.Fullname, Is.EqualTo(_validRequest.Fullname));

            var cccd = await _cccdRepository.GetCccdByCode(_validRequest.CccdCode);
            Assert.That(cccd, Is.Not.Null);
        }

        [Test]
        public void RegisterAsync_ShouldRollbackAndDeleteImagesOnFailure() 
        {
            // Arrange
            _cloudinaryServiceMock
                .Setup(c => c.UploadAsync(It.IsAny<ImageUploadParams>()))
                .Throws(new Exception("Upload failed"));

            _cloudinaryServiceMock
                .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
                .ReturnsAsync(new DeletionResult { Result = "ok" });

            // Act & Assert
            var ex = Assert.Throws<Exception>(() =>
                _userService.RegisterAsync(_validRequest).GetAwaiter().GetResult()); 

            Assert.That(ex.Message, Does.Contain("Đăng ký thất bại"));

            // Verify
            _cloudinaryServiceMock.Verify(
                c => c.DestroyAsync(It.IsAny<DeletionParams>()),
                Times.Exactly(2)
            );
        }
    }
}