using API.Services;
using CloudinaryDotNet.Actions;
using DataAccess.DTOs;
using DataAccess.DTOs.Auth;
using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using DataAccess.Repositories.StationRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Data;
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
            var dbMock = new Moq.Mock<StackExchange.Redis.IDatabase>();
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

            var user = new User
            {
                Email = "user@test.com",
                Fullname = "Test User",
                PasswordHash = "hashed",
                PhoneNumber = "0123456789",
                Dob = new DateTime(1995, 1, 1),
                Gender = true,
                Address = "Some Address",
                RoleId = 1,
                Status = "Active",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            _context.SaveChanges();
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
            var existingUser = new User
            {
                PhoneNumber = "0779132222",
                Email = _validRequest.Email,
                PasswordHash = "dummy_hash",
                Fullname = "Dummy User",
                Dob = DateTime.UtcNow,
                Gender = true,
                Address = "Dummy Address",
                RoleId = 1,
                Status = "Active",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };
            await _userRepository.SaveUser(existingUser);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _userService.RegisterAsync(_validRequest));
            Assert.That(ex.Message, Does.Contain("Email đã tồn tại"));
        }

        [Test]
        public async Task RegisterAsync_ShouldThrowExceptionWhenCccdExists()
        {
            // Arrange: Tạo một người dùng đã tồn tại (để liên kết với CCCD)
            var existingUser = new User
            {
                PhoneNumber = "0779132222",
                Email = "dummy@test.com",
                PasswordHash = "dummy_hash",
                Fullname = "Dummy User",
                Dob = DateTime.UtcNow,
                Gender = true,
                Address = "Dummy Address",
                RoleId = 1,
                Status = "Active",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };
            await _userRepository.SaveUser(existingUser);

            // Arrange: Tạo đối tượng CCCD đã tồn tại và đảm bảo được thêm vào context
            var existingCccd = new Cccd
            {
                Code = _validRequest.CccdCode.Trim(),
                UserId = existingUser.UserId,
                ImgFront = "front.jpg",
                ImgBack = "back.jpg",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            // Thêm đối tượng CCCD vào context và lưu lại
            _context.Cccds.Add(existingCccd);
            await _context.SaveChangesAsync();

            // Act & Assert: Khi đăng ký với CCCD đã tồn tại, sẽ ném InvalidOperationException
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
        public void RegisterAsync_ShouldThrowExceptionWhenPasswordIsWeakLeght()
        {
            // Arrange
            _validRequest.PasswordHash = "weak";

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.RegisterAsync(_validRequest));
            Assert.That(ex.Message, Does.Contain("Mật khẩu phải có từ 8 đến 100 ký tự"));
        }
        [Test]
        public void RegisterAsync_ShouldThrowExceptionWhenPasswordIsWeak()
        {
            // Arrange
            _validRequest.PasswordHash = "weakpassword";

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
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Fullname, Is.EqualTo(_validRequest.Fullname));

            var cccd = await _cccdRepository.GetCccdByCode(_validRequest.CccdCode);
            Assert.That(cccd, Is.Not.Null);
        }

        [Test]
        public void RegisterAsync_ShouldRollbackAndDeleteImagesOnFailure()
        {
            // Arrange

            // Tạo mock cho repository CCCD để ném ra exception khi lưu dữ liệu
            var cccdRepositoryMock = new Mock<ICccdRepository>();
            cccdRepositoryMock
                .Setup(c => c.SaveCccd(It.IsAny<Cccd>()))
                .ThrowsAsync(new Exception("Database error while saving CCCD"));

            // Gán repository CCCD mới dùng cho UserService
            _cccdRepository = cccdRepositoryMock.Object;
            _userService = new UserService(
                _userRepository,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Cấu hình cloudinary để upload thành công cả 2 ảnh
            _cloudinaryServiceMock
                .SetupSequence(c => c.UploadAsync(It.IsAny<ImageUploadParams>()))
                .ReturnsAsync(new ImageUploadResult
                {
                    Url = new Uri("http://mock-url.com/front"),
                    PublicId = "public_id_front"
                })
                .ReturnsAsync(new ImageUploadResult
                {
                    Url = new Uri("http://mock-url.com/back"),
                    PublicId = "public_id_back"
                });

            // Cấu hình phương thức DestroyAsync trả về kết quả thành công (mặc định)
            _cloudinaryServiceMock
                .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
                .ReturnsAsync(new DeletionResult { Result = "ok" });

            // Act & Assert: Khi gọi RegisterAsync, exception sẽ được ném ra do lỗi khi lưu CCCD
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _userService.RegisterAsync(_validRequest));

            Assert.That(ex.Message, Does.Contain("Đăng ký thất bại"));

            _cloudinaryServiceMock.Verify(
                c => c.DestroyAsync(It.IsAny<DeletionParams>()),
                Times.Exactly(2)
            );
        }
        [Test]
        public async Task GetUserByEmail_ShouldReturnUserDto_WhenUserExists()
        {
            // Act
            var result = await _userService.GetUserByEmail("user@test.com");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("user@test.com"));
            Assert.That(result.Fullname, Is.EqualTo("Test User"));
            Assert.That(result.Role, Is.Not.Null);
            Assert.That(result.Role.Name, Is.EqualTo("Driver"));
        }

        [Test]
        public void GetUserByEmail_ShouldThrowArgumentException_WhenUserNotFound()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.GetUserByEmail("nonexistent@test.com"));

            Assert.That(ex.Message, Is.EqualTo("Khong tìm thấy người dùng"));
        }
        [Test]
        public async Task GetUserByCccd_ShouldReturnUserDto_WhenUserExists()
        {
            var cccd = new Cccd
            {
                Code = "0987654321",
                ImgFront = "front.jpg",
                ImgBack = "back.jpg",
                UserId = 1,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };
            _context.Cccds.Add(cccd);
            _context.SaveChanges();
            // Act
            var result = await _userService.GetUserByCccd("0987654321");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("user@test.com"));
            Assert.That(result.Fullname, Is.EqualTo("Test User"));
            Assert.That(result.Role.Name, Is.EqualTo("Driver"));
        }

        [Test]
        public void GetUserByCccd_ShouldThrowArgumentException_WhenUserNotFound()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.GetUserByCccd("nonexistent_cccd"));

            Assert.That(ex.Message, Is.EqualTo("Khong tìm thấy người dùng"));
        }
        [Test]
        public async Task GetUserByPhone_ShouldReturnUserDto_WhenUserExists()
        {
            // Act
            var result = await _userService.GetUserByPhone("0123456789");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("user@test.com"));
            Assert.That(result.Fullname, Is.EqualTo("Test User"));
            Assert.That(result.Role.Name, Is.EqualTo("Driver"));
        }

        [Test]
        public void GetUserByPhone_ShouldThrowArgumentException_WhenUserNotFound()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.GetUserByPhone("0000000000"));

            Assert.That(ex.Message, Is.EqualTo("Khong tìm thấy người dùng"));
        }
        [Test]
        public void ResetPassword_ShouldThrowException_WhenRequestIsNull()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _userService.ResetPassword(null!)
            );

            Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
            Assert.That(ex.InnerException.Message, Is.EqualTo("Request cannot be null"));
        }
        [Test]
        public void ResetPassword_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "notfound@example.com",
                Token = "123456",
                NewPassword = "StrongPassword1!"
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetUserByEmail(request.Email)).ReturnsAsync((User)null!);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _userService.ResetPassword(request)
            );

            Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
            Assert.That(ex.InnerException.Message, Is.EqualTo("User khong ton tai"));
        }
        [Test]
        public void ResetPassword_ShouldThrowException_WhenTokenIsInvalid()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "wrong-token",
                NewPassword = "StrongPassword1!"
            };

            var user = new User
            {
                Email = request.Email,
                PasswordHash = "oldHash"
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetUserByEmail(request.Email)).ReturnsAsync(user);
            var otpServiceMock = new Mock<IOtpServices>();
            otpServiceMock.Setup(o => o.verifyResetPasswordToken(request.Token, request.Email)).ReturnsAsync(false);

            // Create a new instance of UserService with the mocked dependencies
            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                otpServiceMock.Object,
                _licenseRepository,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await userService.ResetPassword(request)
            );

            Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
            Assert.That(ex.InnerException.Message, Is.EqualTo("Invalid Token"));
        }

        [Test]
        public void ResetPassword_ShouldThrowException_WhenPasswordIsWeak()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "123456",
                NewPassword = "weak"
            };

            var user = new User
            {
                Email = request.Email,
                PasswordHash = "oldHash"
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(r => r.GetUserByEmail(request.Email)).ReturnsAsync(user);
            var otpServiceMock = new Mock<IOtpServices>();
            otpServiceMock.Setup(o => o.verifyResetPasswordToken(request.Token, request.Email)).ReturnsAsync(true);

            // Create a new instance of UserService with the mocked dependencies
            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                otpServiceMock.Object,
                _licenseRepository,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await userService.ResetPassword(request)
            );

            Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
            Assert.That(ex.InnerException.Message, Is.EqualTo("Mật khẩu không đủ mạnh"));
        }

        [Test]
        public async Task ResetPassword_ShouldUpdatePassword_WhenValidRequest()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "123456",
                NewPassword = "StrongPassword1!"
            };

            var user = new User
            {
                Email = request.Email,
                PasswordHash = "oldHash"
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            var otpServiceMock = new Mock<IOtpServices>();

            userRepositoryMock.Setup(r => r.GetUserByEmail(request.Email)).ReturnsAsync(user);
            otpServiceMock.Setup(o => o.verifyResetPasswordToken(request.Token, request.Email)).ReturnsAsync(true);

            // Create a new instance of UserService with the mocked dependencies
            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                otpServiceMock.Object,
                _licenseRepository,
                _balanceRepository
            );

            // Act
            var result = await userService.ResetPassword(request);

            // Assert
            Assert.That(result, Is.True);
            userRepositoryMock.Verify(r => r.UpdateUser(It.Is<User>(u =>
                BCrypt.Net.BCrypt.Verify(request.NewPassword, u.PasswordHash, false, BCrypt.Net.HashType.SHA384)
            )), Times.Once);
        }
        [Test]
        public async Task GetProfileByUserId_ShouldReturnProfileDto_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            // Act
            var result = await _userService.GetProfileByUserId(userId);
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Fullname, Is.EqualTo("Test User"));
        }
        [Test]
        public void GetProfileByUserId_ShouldThrowArgumentException_WhenUserNotFound()
        {
            // Arrange
            var userId = 999; // ID không tồn tại
                              // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _userService.GetProfileByUserId(userId));
            Assert.That(ex.Message, Is.EqualTo("Người dùng không tồn tại."));
        }
        [Test]
        public void UpdateUserProfileAsync_ShouldThrowException_WhenRequestIsNull()
        { // Arrange
            var userId = 1;
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.UpdateUserProfileAsync(userId, null!));
            Assert.That(ex.Message, Does.Contain("Request không được null"));
        }

        [Test]
        public void UpgradeProfileAsync_ShouldThrowException_InvalidRequest()
        {
            // Arrange
            var userId = 1;
            RequestProfile request = null!; // Thiết lập request null để kích hoạt exception

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.UpdateUserProfileAsync(userId, request));
            Assert.That(ex.Message, Does.Contain("Request không được null"));
        }
        [Test]
        public void UpgradeProfileAsync_ShouldThrowException_InvalidUserId()
        {
            // Arrange
            var userId = 0; // ID không hợp lệ
            var request = new RequestProfile
            {
                Fullname = "Test User",
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                Dob = new DateTime(1990, 1, 1),
                Gender = true,
                Address = "Test Address"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.UpdateUserProfileAsync(userId, request));
            Assert.That(ex.Message, Does.Contain("Invalid user ID."));
        }
        [Test]
        public void UpgradeProfileAsync_ShouldThrowException_InvalidEmail()
        {
            // Arrange
            var userId = 1;
            var request = new RequestProfile
            {
                Fullname = "Test User",
                Email = "invalid-email", // Email không hợp lệ
                PhoneNumber = "0123456789",
                Dob = new DateTime(1990, 1, 1),
                Gender = true,
                Address = "Test Address"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.UpdateUserProfileAsync(userId, request));
            Assert.That(ex.Message, Does.Contain("Email không hợp lệ"));
        }
        [Test]
        public void ChangePasswordAsync_ShouldThrowArgumentException_WhenPasswordsAreEmpty()
        {
            var passDTO = new ChangePassDTO
            {
                Password = "",
                NewPassword = "",
                ConfirmNewPassword = ""
            };

            var ex = Assert.ThrowsAsync<ArgumentException>(() => _userService.ChangePasswordAsync(passDTO));
            Assert.That(ex.Message, Is.EqualTo("Passwords khong the trong."));
        }

        [Test]
        public void ChangePasswordAsync_ShouldThrowArgumentException_WhenNewPasswordIsNotStrongEnough()
        {
            var passDTO = new ChangePassDTO
            {
                Password = "CurrentPassword123!",
                NewPassword = "weak",
                ConfirmNewPassword = "weak"
            };

            var ex = Assert.ThrowsAsync<ArgumentException>(() => _userService.ChangePasswordAsync(passDTO));
            Assert.That(ex.Message, Is.EqualTo("Password is not strong enough"));
        }

        [Test]
        public void ChangePasswordAsync_ShouldThrowArgumentException_WhenNewPasswordAndConfirmationDoNotMatch()
        {
            var passDTO = new ChangePassDTO
            {
                Password = "CurrentPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmNewPassword = "DifferentPassword123!"
            };

            var ex = Assert.ThrowsAsync<ArgumentException>(() => _userService.ChangePasswordAsync(passDTO));
            Assert.That(ex.Message, Is.EqualTo("New password and confirmation do not match."));
        }

        [Test]
        public void ChangePasswordAsync_ShouldThrowKeyNotFoundException_WhenUserNotFound()
        {
            var passDTO = new ChangePassDTO
            {
                UserId = 1,
                Password = "CurrentPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmNewPassword = "NewPassword123!"
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUserById(It.IsAny<int>())).ReturnsAsync((User)null!);
            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => userService.ChangePasswordAsync(passDTO));
            Assert.That(ex.Message, Is.EqualTo("User not found."));
        }

        [Test]
        public void ChangePasswordAsync_ShouldThrowArgumentException_WhenCurrentPasswordIsIncorrect()
        {
            var passDTO = new ChangePassDTO
            {
                UserId = 1,
                Password = "WrongPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmNewPassword = "NewPassword123!"
            };

            var user = new User { UserId = 1, PasswordHash = BCrypt.Net.BCrypt.HashPassword("CurrentPassword123!") };
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUserById(It.IsAny<int>())).ReturnsAsync(user);
            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await userService.ChangePasswordAsync(passDTO));
            Assert.That(ex.Message, Is.EqualTo("Mật khẩu hiện tại không đúng."));
        }


        [Test]
        public async Task ChangePasswordAsync_ShouldUpdateUserPassword_WhenAllConditionsAreMet()
        {
            var passDTO = new ChangePassDTO
            {
                UserId = 1,
                Password = "CurrentPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmNewPassword = "NewPassword123!"
            };

            var user = new User { UserId = 1, PasswordHash = BCrypt.Net.BCrypt.HashPassword("CurrentPassword123!") };
            var userRepositoryMock = new Mock<IUserRepository>();

            userRepositoryMock.Setup(repo => repo.GetUserById(It.IsAny<int>())).ReturnsAsync(user);
            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            await userService.ChangePasswordAsync(passDTO);

            userRepositoryMock.Verify(repo => repo.UpdateUser(It.Is<User>(u => u.UserId == 1 && BCrypt.Net.BCrypt.Verify("NewPassword123!", u.PasswordHash, false, BCrypt.Net.HashType.SHA384))), Times.Once);
        }
        [Test]
        public async Task GetUsersByEmailOrPhoneAsync_ShouldReturnUsers_WhenUsersExist()
        {
            // Arrange
            var search = "test";
            var users = new List<User>
            {
                new User { UserId = 1, Email = "test1@example.com", PhoneNumber = "1234567890" },
                new User { UserId = 2, Email = "test2@example.com", PhoneNumber = "0987654321" }
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUserByEmailOrPhone(search)).ReturnsAsync(users);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act
            var result = await userService.GetUsersByEmailOrPhoneAsync(search);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Email, Is.EqualTo("test1@example.com"));
            Assert.That(result[1].PhoneNumber, Is.EqualTo("0987654321"));
        }

        [Test]
        public void GetUsersByEmailOrPhoneAsync_ShouldThrowArgumentNullException_WhenSearchIsNull()
        {
            // Arrange
            string invalidSearch = null!;

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _userService.GetUsersByEmailOrPhoneAsync(invalidSearch));

            // Kiểm tra thông điệp hoặc ParamName (tuỳ logic của bạn)
            Assert.That(ex.ParamName, Is.EqualTo("Tìm kiếm không thể trống hoặc khoảng trắng"));
        }
        [Test]
        public async Task GetLicenseByCode_ShouldReturnDriverLicenseDTO_WhenLicenseExists()
        {
            // Arrange
            var code = "123456";
            var existingLicense = new DriverLicense
            {
                DriverLicenseId = 1,
                UserId = 1,
                Code = code,
                Class = "B",
                ImgFront = "front.jpg",
                ImgBack = "back.jpg",
                Status = "Active",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };
            var user = new User
            {
                UserId = 1,
                Fullname = "Test User",
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(code)).ReturnsAsync(existingLicense);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUserById(existingLicense.UserId)).ReturnsAsync(user);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act
            var result = await userService.GetLicenseByCode(code);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.LicenseId, Is.EqualTo(existingLicense.DriverLicenseId));
            Assert.That(result.LicenseNumber, Is.EqualTo(existingLicense.Code));
            Assert.That(result.Class, Is.EqualTo(existingLicense.Class));
            Assert.That(result.FrontImageUrl, Is.EqualTo(existingLicense.ImgFront));
            Assert.That(result.BackImageUrl, Is.EqualTo(existingLicense.ImgBack));
            Assert.That(result.Status, Is.EqualTo(existingLicense.Status));
            Assert.That(result.User.UserId, Is.EqualTo(user.UserId));
            Assert.That(result.User.Fullname, Is.EqualTo(user.Fullname));
            Assert.That(result.User.Email, Is.EqualTo(user.Email));
            Assert.That(result.User.PhoneNumber, Is.EqualTo(user.PhoneNumber));
        }

        [Test]
        public void GetLicenseByCode_ShouldThrowArgumentException_WhenCodeIsNullOrEmpty()
        {
            // Arrange
            string invalidCode = "";

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.GetLicenseByCode(invalidCode));

            Assert.That(ex.ParamName, Is.EqualTo("code"));
            Assert.That(ex.Message, Does.Contain("Mã số bằng lái không hợp lệ"));
        }

        [Test]
        public void GetLicenseByCode_ShouldThrowArgumentException_WhenLicenseNotFound()
        {
            // Arrange
            var code = "123456";
            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(code)).ReturnsAsync((DriverLicense)null!);

            var userService = new UserService(
                _userRepository,
                _cccdRepository,
                _imageService,
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.GetLicenseByCode(code));

            Assert.That(ex.Message, Is.EqualTo("License not found"));
        }

        [Test]
        public void GetLicenseByCode_ShouldThrowArgumentException_WhenUserNotFound()
        {
            // Arrange
            var code = "123456";
            var existingLicense = new DriverLicense
            {
                DriverLicenseId = 1,
                UserId = 1,
                Code = code,
                Class = "B",
                ImgFront = "front.jpg",
                ImgBack = "back.jpg",
                Status = "Active",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(code)).ReturnsAsync(existingLicense);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUserById(existingLicense.UserId)).ReturnsAsync((User)null!);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.GetLicenseByCode(code));

            Assert.That(ex.Message, Is.EqualTo("User not found"));
        }
        [Test]
        public async Task AddDriverLicenseAsync_ShouldAddLicense_WhenValidRequest()
        {
            // Arrange
            var userId = 1;

            var frontFileMock = new Mock<IFormFile>();
            frontFileMock.Setup(f => f.Length).Returns(1024);
            frontFileMock.Setup(f => f.FileName).Returns("front.jpg");

            var backFileMock = new Mock<IFormFile>();
            backFileMock.Setup(f => f.Length).Returns(2048);
            backFileMock.Setup(f => f.FileName).Returns("back.jpg");

            var request = new DriverLicenseRequest
            {
                LicenseNumber = "123456",
                Class = "B",
                LicenseFrontImage = frontFileMock.Object,
                LicenseBackImage = backFileMock.Object
            };

            var user = new User { UserId = userId };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicensesByUserId(userId)).ReturnsAsync(new List<DriverLicense>());
            licenseRepositoryMock.Setup(repo => repo.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());

            // Key fix: Use a URI with an explicit path to avoid trailing slash
            var imageUrl = new Uri("http://mock-url.com/image.jpg");
            var cloudinaryServiceMock = new Mock<ICloudinaryService>();
            cloudinaryServiceMock.Setup(service => service.UploadAsync(It.IsAny<ImageUploadParams>()))
                .ReturnsAsync(new ImageUploadResult { Url = imageUrl, PublicId = "public_id" });

            var userService = new UserService(
                Mock.Of<IUserRepository>(repo => repo.GetUserById(userId) == Task.FromResult(user)),
                _cccdRepository,
                new ImageService(cloudinaryServiceMock.Object),
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act
            await userService.AddDriverLicenseAsync(userId, request);

            // Assert
            licenseRepositoryMock.Verify(repo => repo.SaveLicense(It.Is<DriverLicense>(l =>
                l.UserId == userId &&
                l.Code == "123456" &&
                l.Class == "B" &&
                l.ImgFront == "http://mock-url.com/image.jpg" && // Matches mock URL
                l.ImgBack == "http://mock-url.com/image.jpg" &&
                l.Status == "Active" // Ensure this matches the service's actual status
            )), Times.Once);
        }


        [Test]
        public void AddDriverLicenseAsync_ShouldThrowArgumentException_WhenRequestIsNull()
        {
            // Arrange
            var userId = 1;
            DriverLicenseRequest request = null!;

            var userService = new UserService(
                _userRepository,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.AddDriverLicenseAsync(userId, request));

            Assert.That(ex.Message, Is.EqualTo("Request cannot be null"));
        }

        [Test]
        public void AddDriverLicenseAsync_ShouldThrowArgumentException_WhenUserNotFound()
        {
            // Arrange
            var userId = 1;
            var request = new DriverLicenseRequest
            {
                LicenseNumber = "123456",
                Class = "B",
                LicenseFrontImage = new Mock<IFormFile>().Object,
                LicenseBackImage = new Mock<IFormFile>().Object
            };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUserById(userId)).ReturnsAsync((User)null!);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.AddDriverLicenseAsync(userId, request));

            Assert.That(ex.Message, Is.EqualTo("User not found"));
        }

        [Test]
        public async Task AddDriverLicenseAsync_ShouldUpdateLicense_WhenLicenseExistsAndIsInactive()
        {
            // Arrange
            var userId = 1;

            var frontFileMock = new Mock<IFormFile>();
            frontFileMock.Setup(f => f.Length).Returns(1024);
            frontFileMock.Setup(f => f.FileName).Returns("front.jpg");

            var backFileMock = new Mock<IFormFile>();
            backFileMock.Setup(f => f.Length).Returns(2048);
            backFileMock.Setup(f => f.FileName).Returns("back.jpg");

            var request = new DriverLicenseRequest
            {
                LicenseNumber = "123456",
                Class = "B",
                LicenseFrontImage = frontFileMock.Object,
                LicenseBackImage = backFileMock.Object
            };

            var user = new User { UserId = userId };

            var existingLicense = new DriverLicense
            {
                UserId = userId,
                Code = request.LicenseNumber,
                Status = "Inactive",
                ImgFrontPubblicId = "old_front_id",
                ImgBackPubblicId = "old_back_id"
            };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicensesByUserId(userId))
                                 .ReturnsAsync(new List<DriverLicense> { existingLicense });
            licenseRepositoryMock.Setup(repo => repo.BeginTransactionAsync())
                                 .ReturnsAsync(Mock.Of<IDbContextTransaction>());

            var imageUrl = new Uri("http://mock-url.com/image.jpg");
            var imageServiceMock = new Mock<ICloudinaryService>();
            imageServiceMock.Setup(service => service.UploadAsync(It.IsAny<ImageUploadParams>()))
                  .ReturnsAsync(new ImageUploadResult { Url = imageUrl, PublicId = "public_id" });
            imageServiceMock.Setup(service => service.DestroyAsync(It.IsAny<DeletionParams>()))
                 .ReturnsAsync(new DeletionResult { Result = "ok" });

            var userService = new UserService(
                Mock.Of<IUserRepository>(repo => repo.GetUserById(userId) == Task.FromResult(user)),
                _cccdRepository,
                new ImageService(imageServiceMock.Object), // Wrap the mock in ImageService
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act
            await userService.AddDriverLicenseAsync(userId, request);

            // Assert
            licenseRepositoryMock.Verify(repo => repo.UpdateLicense(It.Is<DriverLicense>(l =>
                l.UserId == userId &&
                l.Code == "123456" &&
                l.Class == "B" &&
                l.ImgFront == "http://mock-url.com/image.jpg" &&
                l.ImgBack == "http://mock-url.com/image.jpg" &&
                l.Status == "Active"
            )), Times.Once);

            imageServiceMock.Verify(service =>
                service.DestroyAsync(It.Is<DeletionParams>(p => p.PublicId == "old_front_id")), Times.Once);
            imageServiceMock.Verify(service =>
                service.DestroyAsync(It.Is<DeletionParams>(p => p.PublicId == "old_back_id")), Times.Once);
        }

        [Test]
        public void AddDriverLicenseAsync_ShouldThrowArgumentException_WhenLicenseExistsAndIsActive()
        {
            // Arrange
            var userId = 1;
            var request = new DriverLicenseRequest
            {
                LicenseNumber = "123456",
                Class = "B",
                LicenseFrontImage = new Mock<IFormFile>().Object,
                LicenseBackImage = new Mock<IFormFile>().Object
            };

            var user = new User
            {
                UserId = userId,
                Fullname = "Test User",
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            };

            var existingLicense = new DriverLicense
            {
                UserId = userId,
                Code = request.LicenseNumber,
                Status = "Active"
            };

            var existingLicenses = new List<DriverLicense> { existingLicense };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUserById(userId)).ReturnsAsync(user);

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicensesByUserId(userId)).ReturnsAsync(existingLicenses);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.AddDriverLicenseAsync(userId, request));

            Assert.That(ex.Message, Is.EqualTo("License already exists"));
        }

        [Test]
        public async Task UpdateDriverLiscense_ShouldUpdateLicense_WhenValidRequest()
        {
            // Arrange
            var licenseCode = "123456";
            var frontFileMock = new Mock<IFormFile>();
            var backFileMock = new Mock<IFormFile>();

            frontFileMock.Setup(f => f.Length).Returns(1024);
            frontFileMock.Setup(f => f.FileName).Returns("front.jpg");
            frontFileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024]));
            frontFileMock.Setup(f => f.ContentType).Returns("image/jpeg");

            backFileMock.Setup(f => f.Length).Returns(2048);
            backFileMock.Setup(f => f.FileName).Returns("back.jpg");
            backFileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[2048]));
            backFileMock.Setup(f => f.ContentType).Returns("image/jpeg");

            var request = new DriverLicenseRequest
            {
                LicenseNumber = "654321",
                Class = "B",
                LicenseFrontImage = frontFileMock.Object,
                LicenseBackImage = backFileMock.Object
            };

            var existingLicense = new DriverLicense
            {
                Code = licenseCode,
                ImgFrontPubblicId = "old_front_id",
                ImgBackPubblicId = "old_back_id"
            };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(licenseCode)).ReturnsAsync(existingLicense);
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(request.LicenseNumber)).ReturnsAsync((DriverLicense)null!);
            licenseRepositoryMock.Setup(repo => repo.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());

            var imageServiceMock = new Mock<ICloudinaryService>();
            imageServiceMock.Setup(service => service.UploadAsync(It.IsAny<ImageUploadParams>()))
                .ReturnsAsync(new ImageUploadResult { Url = new Uri("http://mock-url.com/image.jpg"), PublicId = "public_id" });
            imageServiceMock.Setup(service => service.DestroyAsync(It.IsAny<DeletionParams>()))
                .ReturnsAsync(new DeletionResult { Result = "ok" });

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                Mock.Of<ICccdRepository>(),
                new ImageService(imageServiceMock.Object),
                Mock.Of<IOtpServices>(),
                licenseRepositoryMock.Object,
                Mock.Of<IBalancement>()
            );

            // Act
            await userService.UpdateDriverLiscense(licenseCode, request);

            // Assert
            licenseRepositoryMock.Verify(repo => repo.UpdateLicense(It.Is<DriverLicense>(l =>
                l.Code == request.LicenseNumber &&
                l.Class == request.Class &&
                l.ImgFront == "http://mock-url.com/image.jpg" &&
                l.ImgBack == "http://mock-url.com/image.jpg"
            )), Times.Once);

            imageServiceMock.Verify(service => service.DestroyAsync(It.Is<DeletionParams>(p => p.PublicId == "old_front_id")), Times.Once);
            imageServiceMock.Verify(service => service.DestroyAsync(It.Is<DeletionParams>(p => p.PublicId == "old_back_id")), Times.Once);
        }
       [Test]
        public void UpdateDriverLiscense_ShouldThrowException_WhenRequestIsNull()
        {
            // Arrange
            var licenseCode = "123456";
            DriverLicenseRequest request = null!;

            var cloudinaryServiceMock = new Mock<ICloudinaryService>();
            var imageService = new ImageService(cloudinaryServiceMock.Object);

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                Mock.Of<ICccdRepository>(),
                imageService,
                Mock.Of<IOtpServices>(),
                Mock.Of<IDriverLicenseRepository>(),
                Mock.Of<IBalancement>()
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.UpdateDriverLiscense(licenseCode, request));

            Assert.That(ex.Message, Is.EqualTo("Request cannot be null"));
        }




        [Test]
        public void UpdateDriverLiscense_ShouldThrowException_WhenLicenseNotFound()
        {
            // Arrange
            var licenseCode = "123456";
            var request = new DriverLicenseRequest
            {
                LicenseNumber = "654321",
                Class = "B",
                LicenseFrontImage = new Mock<IFormFile>().Object,
                LicenseBackImage = new Mock<IFormFile>().Object
            };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(licenseCode)).ReturnsAsync((DriverLicense)null!);

            var cloudinaryServiceMock = new Mock<ICloudinaryService>();
            var imageService = new ImageService(cloudinaryServiceMock.Object);

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                Mock.Of<ICccdRepository>(),
                imageService,
                Mock.Of<IOtpServices>(),
                licenseRepositoryMock.Object,
                Mock.Of<IBalancement>()
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.UpdateDriverLiscense(licenseCode, request));

            Assert.That(ex.Message, Is.EqualTo("License not found"));
        }

        [Test]
        public void UpdateDriverLiscense_ShouldThrowException_WhenLicenseNumberExists()
        {
            // Arrange
            var licenseCode = "123456";
            var request = new DriverLicenseRequest
            {
                LicenseNumber = "654321",
                Class = "B",
                LicenseFrontImage = new Mock<IFormFile>().Object,
                LicenseBackImage = new Mock<IFormFile>().Object
            };

            var existingLicense = new DriverLicense
            {
                Code = "654321"
            };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(licenseCode)).ReturnsAsync(new DriverLicense());
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(request.LicenseNumber)).ReturnsAsync(existingLicense);

            var cloudinaryServiceMock = new Mock<ICloudinaryService>();
            var imageService = new ImageService(cloudinaryServiceMock.Object);

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                Mock.Of<ICccdRepository>(),
                imageService,
                Mock.Of<IOtpServices>(),
                licenseRepositoryMock.Object,
                Mock.Of<IBalancement>()
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.UpdateDriverLiscense(licenseCode, request));

            Assert.That(ex.Message, Is.EqualTo("License mà bạn muốn cập nhật đã tồn tại trong hệ thống"));
        }

        [Test]
        public async Task DeleteDriverLicenseAsync_ShouldSetLicenseToInactive_WhenLicenseExists()
        {
            // Arrange
            var licenseCode = "123456";
            var existingLicense = new DriverLicense
            {
                DriverLicenseId = 1,
                UserId = 1,
                Code = licenseCode,
                Status = "Active",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(licenseCode)).ReturnsAsync(existingLicense);
            licenseRepositoryMock.Setup(repo => repo.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());

            var userService = new UserService(
                _userRepository,
                _cccdRepository,
                _imageService,
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act
            await userService.DeleteDriverLicenseAsync(licenseCode);

            // Assert
            licenseRepositoryMock.Verify(repo => repo.UpdateLicense(It.Is<DriverLicense>(l =>
                l.Code == licenseCode &&
                l.Status == "Inactive" &&
                l.UpdateAt != null
            )), Times.Once);
        }

        [Test]
        public void DeleteDriverLicenseAsync_ShouldThrowArgumentException_WhenLicenseNotFound()
        {
            // Arrange
            var licenseCode = "123456";
            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(licenseCode)).ReturnsAsync((DriverLicense)null!);

            var userService = new UserService(
                _userRepository,
                _cccdRepository,
                _imageService,
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.DeleteDriverLicenseAsync(licenseCode));

            Assert.That(ex.Message, Is.EqualTo("License not found"));
        }

        [Test]
        public async Task ActiveDriverLicenseAsync_ShouldSetLicenseToActive_WhenLicenseExists()
        {
            // Arrange
            var licenseCode = "123456";
            var existingLicense = new DriverLicense
            {
                DriverLicenseId = 1,
                UserId = 1,
                Code = licenseCode,
                Status = "Inactive",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(licenseCode)).ReturnsAsync(existingLicense);
            licenseRepositoryMock.Setup(repo => repo.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());

            var userService = new UserService(
                _userRepository,
                _cccdRepository,
                _imageService,
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act
            await userService.ActiveDriverLicenseAsync(licenseCode);

            // Assert
            licenseRepositoryMock.Verify(repo => repo.UpdateLicense(It.Is<DriverLicense>(l =>
                l.Code == licenseCode &&
                l.Status == "Active" &&
                l.UpdateAt != null
            )), Times.Once);
        }

        [Test]
        public void ActiveDriverLicenseAsync_ShouldThrowArgumentException_WhenLicenseNotFound()
        {
            // Arrange
            var licenseCode = "123456";
            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicenseByCode(licenseCode)).ReturnsAsync((DriverLicense)null!);

            var userService = new UserService(
                _userRepository,
                _cccdRepository,
                _imageService,
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.ActiveDriverLicenseAsync(licenseCode));

            Assert.That(ex.Message, Is.EqualTo("License not found"));
        }
        [Test]
        public void GetActiveDriverLicensesAsync_ShouldReturnActiveLicenses_WhenLicensesExist()
        {
            // Arrange
            var userId = 2;
            var user = new User
            {
                UserId = userId,
                Fullname = "Test User",
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            };

            var licenses = new List<DriverLicense>
            {
                new DriverLicense
                {
                    DriverLicenseId = 2,
                    UserId = userId,
                    Code = "123456",
                    Class = "A",
                    Status = "Active",
                    ImgFront = "http://mock-url.com/front1.jpg",
                    ImgBack = "http://mock-url.com/back1.jpg",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                    User = user
                },
                new DriverLicense
                {
                    DriverLicenseId = 3,
                    UserId = userId,
                    Class = "A",
                    Code = "654321",
                    Status = "Inactive",
                    ImgFront = "http://mock-url.com/front2.jpg",
                    ImgBack = "http://mock-url.com/back2.jpg",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                    User = user
                },
                new DriverLicense
                {
                    DriverLicenseId = 4,
                    UserId = userId,
                    Class = "B",
                    Code = "789012",
                    Status = "Active",
                    ImgFront = "http://mock-url.com/front3.jpg",
                    ImgBack = "http://mock-url.com/back3.jpg",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                    User = user
                }
            };

            // Detach any existing tracked entities
            _context.ChangeTracker.Clear();

            _context.Users.Add(user);
            _context.DriverLicenses.AddRange(licenses);
            _context.SaveChanges();

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicensesByUserId(userId)).ReturnsAsync(licenses);

       

            // Act
            var result = _context.DriverLicenses.Where(c=>c.Status=="Active");


            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }



        [Test]
        public async Task GetActiveDriverLicensesAsync_ShouldReturnEmpty_WhenNoLicensesExist()
        {
            // Arrange
            var userId = 1;
            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetLicensesByUserId(userId)).ReturnsAsync((IEnumerable<DriverLicense>)null!);

            var userService = new UserService(
                _userRepository,
                _cccdRepository,
                _imageService,
                _otpServices,
                licenseRepositoryMock.Object,
                _balanceRepository
            );

            // Act
            var result = await userService.GetActiveDriverLicensesAsync(userId);

            // Assert
            Assert.That(result, Is.Empty);
        }


        [Test]
        public async Task GetLicenseList_ShouldReturnPagedResult_WhenCalledWithValidParameters()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var filter = new DriverLicenseFilter
            {
                Code = "123456",
                Fullname = "Test User",
                Status = "Active",
                Class = "B"
            };

            var pagedResult = new PagedResultD<DriverLicenseDTO>
            {
                TotalCount = 1,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = new List<DriverLicenseDTO>
        {
            new DriverLicenseDTO
            {
                LicenseId = 1,
                LicenseNumber = "123456",
                Class = "B",
                FrontImageUrl = "http://mock-url.com/front.jpg",
                BackImageUrl = "http://mock-url.com/back.jpg",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                User = new UserSimpleDTO
                {
                    UserId = 1,
                    Fullname = "Test User",
                    Email = "test@example.com",
                    PhoneNumber = "1234567890"
                }
            }
        }
            };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetPagedLicensesAsync(pageNumber, pageSize, filter))
                                 .ReturnsAsync(pagedResult);

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                Mock.Of<ICccdRepository>(),
                new ImageService(Mock.Of<ICloudinaryService>()), // Use actual instance with mock dependency
                Mock.Of<IOtpServices>(),
                licenseRepositoryMock.Object,
                Mock.Of<IBalancement>()
            );

            // Act
            var result = await userService.GetLicenseList(pageNumber, pageSize, filter);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.PageNumber, Is.EqualTo(pageNumber));
            Assert.That(result.PageSize, Is.EqualTo(pageSize));
            Assert.That(result.TotalPages, Is.EqualTo(1));
            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Items.First().LicenseNumber, Is.EqualTo("123456"));
        }

        [Test]
        public async Task GetLicenseList_ShouldReturnEmptyPagedResult_WhenNoLicensesExist()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var filter = new DriverLicenseFilter
            {
                Code = "nonexistent",
                Fullname = "Nonexistent User",
                Status = "Inactive",
                Class = "C"
            };

            var pagedResult = new PagedResultD<DriverLicenseDTO>
            {
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = new List<DriverLicenseDTO>()
            };

            var licenseRepositoryMock = new Mock<IDriverLicenseRepository>();
            licenseRepositoryMock.Setup(repo => repo.GetPagedLicensesAsync(pageNumber, pageSize, filter))
                                 .ReturnsAsync(pagedResult);

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                Mock.Of<ICccdRepository>(),
                new ImageService(Mock.Of<ICloudinaryService>()), // Use actual instance with mock dependency
                Mock.Of<IOtpServices>(),
                licenseRepositoryMock.Object,
                Mock.Of<IBalancement>()
            );

            // Act
            var result = await userService.GetLicenseList(pageNumber, pageSize, filter);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalCount, Is.EqualTo(0));
            Assert.That(result.PageNumber, Is.EqualTo(pageNumber));
            Assert.That(result.PageSize, Is.EqualTo(pageSize));
            Assert.That(result.TotalPages, Is.EqualTo(0));
            Assert.That(result.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetLicenseList_ShouldThrowArgumentException_WhenPageNumberIsInvalid()
        {
            // Arrange
            var pageNumber = 0; // Invalid page number
            var pageSize = 10;
            var filter = new DriverLicenseFilter
            {
                Code = "123456",
                Fullname = "Test User",
                Status = "Active",
                Class = "B"
            };

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                Mock.Of<ICccdRepository>(),
                new ImageService(Mock.Of<ICloudinaryService>()), // Use actual instance with mock dependency
                Mock.Of<IOtpServices>(),
                Mock.Of<IDriverLicenseRepository>(),
                Mock.Of<IBalancement>()
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.GetLicenseList(pageNumber, pageSize, filter));
            Assert.That(ex.Message, Does.Contain("Page number must be greater than zero"));
        }





        [Test]
        public void GetLicenseList_ShouldThrowArgumentException_WhenPageSizeIsInvalid()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 0; // Invalid page size
            var filter = new DriverLicenseFilter
            {
                Code = "123456",
                Fullname = "Test User",
                Status = "Active",
                Class = "B"
            };

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                Mock.Of<ICccdRepository>(),
                new ImageService(Mock.Of<ICloudinaryService>()), 
                Mock.Of<IOtpServices>(),
                Mock.Of<IDriverLicenseRepository>(),
                Mock.Of<IBalancement>()
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.GetLicenseList(pageNumber, pageSize, filter));
            Assert.That(ex.Message, Does.Contain("Page size must be greater than zero"));
        }
        [Test]
        public void GetUsers_ShouldReturnPagedResult()
        {
            // Arrange
            var users = new List<UserDto>
                {
                    new UserDto { UserId = 1, Fullname = "John Doe" },
                    new UserDto { UserId = 2, Fullname = "Jane Doe" }
                };
            var pagedResult = new PagedResult<UserDto>(users, 2, 1);
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUsers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(pagedResult);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act
            var result = userService.GetUsers(null, null, null, 1, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data.Count, Is.EqualTo(2));
            Assert.That(result.TotalPages, Is.EqualTo(2));
        }
        [Test]
        public void GetUsers_ShouldReturnPagedResult_WhenCalledWithValidParameters()
        {
            // Arrange
            var searchQuery = "test";
            var status = "Active";
            var roleId = 1;
            var pageNumber = 1;
            var pageSize = 10;

            var users = new List<UserDto>
    {
        new UserDto { UserId = 1, Fullname = "John Doe", Status = "Active" },
        new UserDto { UserId = 2, Fullname = "Jane Doe", Status = "Active" }
    };
            var pagedResult = new PagedResult<UserDto>(users, 2, pageSize);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUsers(searchQuery, status, roleId, pageNumber, pageSize))
                .Returns(pagedResult);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act
            var result = userService.GetUsers(searchQuery, status, roleId, pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data.Count, Is.EqualTo(2));
            Assert.That(result.TotalPages, Is.EqualTo(1));
        }

        [Test]
        public void GetUsers_ShouldReturnEmptyPagedResult_WhenNoUsersExist()
        {
            // Arrange
            var searchQuery = "nonexistent";
            var status = "Inactive";
            var roleId = 1;
            var pageNumber = 1;
            var pageSize = 10;

            var pagedResult = new PagedResult<UserDto>(new List<UserDto>(), 0, pageSize);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetUsers(searchQuery, status, roleId, pageNumber, pageSize))
                .Returns(pagedResult);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act
            var result = userService.GetUsers(searchQuery, status, roleId, pageNumber, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data.Count, Is.EqualTo(0));
            Assert.That(result.TotalPages, Is.EqualTo(0));
        }

        [Test]
        public void GetUsers_ShouldThrowArgumentException_WhenPageNumberIsInvalid()
        {
            // Arrange
            var searchQuery = "test";
            var status = "Active";
            var roleId = 1;
            var pageNumber = 0; // Invalid page number
            var pageSize = 10;

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                userService.GetUsers(searchQuery, status, roleId, pageNumber, pageSize));
            Assert.That(ex.Message, Does.Contain("Page number must be greater than zero"));
        }

        [Test]
        public void GetUsers_ShouldThrowArgumentException_WhenPageSizeIsInvalid()
        {
            // Arrange
            var searchQuery = "test";
            var status = "Active";
            var roleId = 1;
            var pageNumber = 1;
            var pageSize = 0; // Invalid page size

            var userRepositoryMock = new Mock<IUserRepository>();
            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                userService.GetUsers(searchQuery, status, roleId, pageNumber, pageSize));
            Assert.That(ex.Message, Does.Contain("Page size must be greater than zero"));
        }


        [Test]
        public async Task ChangeUserStatusAsync_ShouldInvokeRepositoryChangeUserStatus()
        {
            // Arrange: Tạo userId và newStatus mẫu
            var userId = 1;
            var newStatus = "Inactive";

            // Tạo mock cho IUserRepository và setup phương thức ChangeUserStatusAsync trả về Task.CompletedTask
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(repo => repo.ChangeUserStatusAsync(userId, newStatus))
                .Returns(Task.CompletedTask);

            // Tạo instance của UserService với repository được mock
            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act: Gọi phương thức ChangeUserStatusAsync của UserService
            await userService.ChangeUserStatusAsync(userId, newStatus);

            // Assert: Xác nhận rằng phương thức ChangeUserStatusAsync trên repository đã được gọi đúng 1 lần với userId và newStatus mong đợi.
            userRepositoryMock.Verify(repo => repo.ChangeUserStatusAsync(userId, newStatus), Times.Once);
        }
        [Test]
        public void GetFeedbacks_ShouldReturnPagedResult_WhenCalledWithValidParameters()
        {
            // Arrange
            var search = "test";
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 12, 31);
            var page = 1;
            var pageSize = 10;

            var feedbacks = new List<FeedbackDto>
                {
                    new FeedbackDto { Id = 1, User = "User1", Message = "Feedback1", Date = new DateTime(2023, 6, 1) },
                    new FeedbackDto { Id = 2, User = "User2", Message = "Feedback2", Date = new DateTime(2023, 7, 1) }
                };
            var pagedResult = new PagedResult<FeedbackDto>(feedbacks, 2, pageSize);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetFeedbacks(search, startDate, endDate, page, pageSize))
                              .Returns(pagedResult);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act
            var result = userService.GetFeedbacks(search, startDate, endDate, page, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data.Count, Is.EqualTo(2));
            Assert.That(result.TotalPages, Is.EqualTo(1));
            Assert.That(result.Data.First().Message, Is.EqualTo("Feedback1"));
        }

        [Test]
        public void GetFeedbacks_ShouldReturnEmptyPagedResult_WhenNoFeedbacksExist()
        {
            // Arrange
            var search = "nonexistent";
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 12, 31);
            var page = 1;
            var pageSize = 10;

            var pagedResult = new PagedResult<FeedbackDto>(new List<FeedbackDto>(), 0, pageSize);

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetFeedbacks(search, startDate, endDate, page, pageSize))
                              .Returns(pagedResult);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act
            var result = userService.GetFeedbacks(search, startDate, endDate, page, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data.Count, Is.EqualTo(0));
            Assert.That(result.TotalPages, Is.EqualTo(0));
        }

        [Test]
        public void GetFeedbacks_ShouldThrowArgumentException_WhenPageNumberIsInvalid()
        {
            // Arrange
            var search = "test";
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 12, 31);
            var page = 0; // Invalid page number
            var pageSize = 10;

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                userService.GetFeedbacks(search, startDate, endDate, page, pageSize));
            Assert.That(ex.Message, Does.Contain("Page number must be greater than zero"));
        }

        [Test]
        public void GetFeedbacks_ShouldThrowArgumentException_WhenPageSizeIsInvalid()
        {
            // Arrange
            var search = "test";
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 12, 31);
            var page = 1;
            var pageSize = 0; // Invalid page size

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                userService.GetFeedbacks(search, startDate, endDate, page, pageSize));
            Assert.That(ex.Message, Does.Contain("Page size must be greater than zero"));
        }

        [Test]
        public async Task GetFeedbackByUserId_ShouldReturnFeedbacks_WhenUserIdIsValid()
        {
            // Arrange
            var userId = 1;
            var feedbacks = new List<Feedback>
    {
        new Feedback { FeedbackId = 1, UserId = userId, Message = "Feedback 1", CreatedAt = DateTime.UtcNow },
        new Feedback { FeedbackId = 2, UserId = userId, Message = "Feedback 2", CreatedAt = DateTime.UtcNow }
    };

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetFeedbackByUserId(userId)).ReturnsAsync(feedbacks);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act
            var result = await userService.GetFeedbackByUserId(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.First().Message, Is.EqualTo("Feedback 1"));
        }

        [Test]
        public void GetFeedbackByUserId_ShouldThrowArgumentException_WhenUserIdIsInvalid()
        {
            // Arrange
            var invalidUserId = 0;

            var userService = new UserService(
                Mock.Of<IUserRepository>(),
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await userService.GetFeedbackByUserId(invalidUserId));
            Assert.That(ex.Message, Is.EqualTo("Invalid user ID."));
        }

        [Test]
        public async Task GetFeedbackByUserId_ShouldReturnEmptyList_WhenNoFeedbacksExist()
        {
            // Arrange
            var userId = 1;
            var feedbacks = new List<Feedback>();

            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(repo => repo.GetFeedbackByUserId(userId)).ReturnsAsync(feedbacks);

            var userService = new UserService(
                userRepositoryMock.Object,
                _cccdRepository,
                _imageService,
                _otpServices,
                _licenseRepository,
                _balanceRepository
            );

            // Act
            var result = await userService.GetFeedbackByUserId(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

    }
}