using DataAccess.Models;
using DataAccess.Repositories;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;

namespace TestProject.FeedbackTest
{
    public class FeedbackRepoTest
    {
        private WccsContext _context;
        private FeedbackRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

            _context = new WccsContext(options);
            _repository = new FeedbackRepository(_context);
            SeedData();
        }

        private void SeedData()
        {
            // Khởi tạo các đối tượng Role
            var role = new Role { RoleId = 1, RoleName = "Driver" };

            // Khởi tạo các đối tượng User
            var user1 = new User
            {
                UserId = 1,
                RoleId = role.RoleId,
                Fullname = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890",
                Dob = DateTime.Parse("1985-01-01"),
                Gender = true,
                Address = "123 Main St",
                PasswordHash = "hashedpassword",
                Status = "Active",
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                Role = role
            };

            var user2 = new User
            {
                UserId = 2,
                RoleId = role.RoleId,
                Fullname = "Jane Smith",
                Email = "jane.smith@example.com",
                PhoneNumber = "0987654321",
                Dob = DateTime.Parse("1990-02-02"),
                Gender = false,
                Address = "456 Another St",
                PasswordHash = "hashedpassword2",
                Status = "Active",
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                Role = role
            };

            // Thêm User vào DbContext
            _context.Users.AddRange(user1, user2);

            // Khởi tạo các đối tượng Feedback
            var feedback1 = new Feedback
            {
                FeedbackId = 1,
                UserId = user1.UserId,
                Type = "Car",
                Message = "Bad battery",
                Status = "Resolved",
                CreatedAt = DateTime.Now
            };

            var feedback2 = new Feedback
            {
                FeedbackId = 2,
                UserId = user2.UserId,
                Type = "ChargingStation",
                Message = "Slow charging",
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            // Thêm Feedback vào DbContext
            _context.Feedbacks.AddRange(feedback1, feedback2);

            _context.SaveChanges();
        }


        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void GetFeedbacks_FilterByMessage_ReturnsCorrectResult()
        {
            // Act
            var result = _repository.GetFeedbacks("battery", null, null, null, null, 1, 10);

            // Assert
            Assert.That(result.Data.Count, Is.EqualTo(1)); // Chỉ có 1 feedback chứa từ "battery" trong message
            Assert.That(result.Data[0].Message, Is.EqualTo("Bad battery"));
        }


        [Test]
        public void GetFeedbacks_FilterByType_ReturnsPagedResult()
        {
            // Act
            var result = _repository.GetFeedbacks(null, "Car", null, null, null, 1, 10);

            // Assert
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data[0].Message, Is.EqualTo("Bad battery")); 
            Assert.That(result.TotalPages, Is.EqualTo(1)); 
        }

        [Test]
        public void GetFeedbacks_FilterByStatus_ReturnsCorrectResult()
        {
            // Act
            var result = _repository.GetFeedbacks(null, null, "Resolved", null, null, 1, 10);

            // Assert
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data[0].Message, Is.EqualTo("Bad battery"));
        }

        [Test]
        public void GetFeedbacks_WithPagingAndFilter_ReturnsPagedResult()
        {
            // Act
            var result = _repository.GetFeedbacks(null, "Car", null, null, null, 1, 1);

            // Assert
            Assert.That(result.Data.Count, Is.EqualTo(1)); // Chỉ có 1 feedback được trả về
            Assert.That(result.TotalPages, Is.EqualTo(1)); // Kiểm tra tổng số trang
        }

        [Test]
        public void GetFeedbacks_FilterByDate_ReturnsCorrectResult()
        {
            // Tạo ngày bắt đầu và kết thúc
            var startDate = DateTime.Now.AddDays(-2); // 1 ngày trước
            var endDate = DateTime.Now.AddDays(2); // 1 ngày sau

            // Act
            var result = _repository.GetFeedbacks(null, null, null, startDate, endDate, 1, 10);

            // Assert
            Assert.That(result.Data.Count, Is.EqualTo(2)); // Lọc toàn bộ feedbacks trong phạm vi ngày
        }

        [Test]
        public void AddFeedback_AddsFeedbackToContext()
        {
            var feedback = new Feedback
            {
                FeedbackId = 3,
                UserId = 1,
                Type = "Car",
                Message = "Another Feedback",
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _repository.AddFeedback(feedback);
            var all = _context.Feedbacks.ToList();

            Assert.That(all.Count, Is.EqualTo(3)); 
        }

        [Test]
        public void AddFeedback_NullFeedback_ThrowsException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _repository.AddFeedback(null);
            });
        }

        [Test]
        public async Task GetByIdAsync_ReturnsCorrectFeedback()
        {
            var result = await _repository.GetByIdAsync(1);

            Assert.That(result?.Message, Is.EqualTo("Bad battery"));
        }

        [Test]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(999); // Id không tồn tại

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task SaveChangesAsync_PersistsChanges()
        {
            var feedback = await _repository.GetByIdAsync(1);
            feedback.Status = "Resolved";

            await _repository.SaveChangesAsync();
            var updated = await _repository.GetByIdAsync(1);

            Assert.That(updated?.Status, Is.EqualTo("Resolved")); // Kiểm tra trạng thái được cập nhật
        }

    }
}
