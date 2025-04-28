using API.Services;
using DataAccess.DTOs.UserDTO;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TestProject.FeedbackTest
{
    public class FeedbackServiceTest
    {
        private WccsContext _context;
        private FeedbackService _feedbackService;
        private FeedbackRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

            _context = new WccsContext(options);
            _repository = new FeedbackRepository(_context);
            _feedbackService = new FeedbackService(_repository);
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
                Status = "Processed",
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
        public void GetFeedbacks_FilterByType_ReturnsCorrectResult()
        {
            // Act
            var result = _feedbackService.GetFeedbacks(null, "Car", null, null, null, 1, 10);

            // Assert
            Assert.That(result.Data.Count, Is.EqualTo(1)); // Chỉ có 1 feedback với loại "Car"
            Assert.That(result.Data[0].Message, Is.EqualTo("Bad battery"));
        }

        [Test]
        public void GetFeedbacks_FilterByStatus_ReturnsCorrectResult()
        {
            // Act
            var result = _feedbackService.GetFeedbacks(null, null, "Processed", null, null, 1, 10);

            // Assert
            Assert.That(result.Data.Count, Is.EqualTo(1)); // Chỉ có 1 feedback với trạng thái "Resolved"
            Assert.That(result.Data[0].Message, Is.EqualTo("Bad battery"));
        }

        [Test]
        public void GetFeedbacks_FilterByMessage_ReturnsCorrectResult()
        {
            // Act
            var result = _feedbackService.GetFeedbacks("battery", null, null, null, null, 1, 10);

            // Assert
            Assert.That(result.Data.Count, Is.EqualTo(1)); // Chỉ có 1 feedback chứa từ "battery" trong message
            Assert.That(result.Data[0].Message, Is.EqualTo("Bad battery"));
        }

        [Test]
        public void GetFeedbacks_WithPaging_ReturnsPagedResult()
        {
            // Act
            var result = _feedbackService.GetFeedbacks(null, null, null, null, null, 1, 1);

            // Assert
            Assert.That(result.Data.Count, Is.EqualTo(1)); // Phân trang 1 kết quả
            Assert.That(result.TotalPages, Is.EqualTo(2)); // Tổng cộng có 2 trang
        }


        [Test]
        public void AddFeedback_AddsFeedbackWithCarSuccessfully()
        {
            var dto = new AddFeedbackDto
            {
                UserId = 1,
                Message = "Test message",
                Type = "Car",
                CarId = 1,
                StationId = null,
                PointId = null
            };

            // Act
            _feedbackService.AddFeedback(dto);
            var allFeedbacks = _context.Feedbacks.ToList();

            // Assert
            Assert.That(allFeedbacks.Count, Is.EqualTo(3)); // Đảm bảo feedback đã được thêm
            Assert.That(allFeedbacks[2].Message, Is.EqualTo("Test message"));
        }

        [Test]
        public void AddFeedback_AddsFeedbackWithStationSuccessfully()
        {
            var dto = new AddFeedbackDto
            {
                UserId = 2,
                Message = "Test station feedback",
                Type = "ChargingStation",
                CarId = null,
                StationId = 1,
                PointId = null
            };

            // Act
            _feedbackService.AddFeedback(dto);
            var addedFeedback = _context.Feedbacks.Last();
            var allFeedbacks = _context.Feedbacks.ToList();

            // Assert
            Assert.That(allFeedbacks.Count, Is.EqualTo(3));
            Assert.That(addedFeedback.Message, Is.EqualTo("Test station feedback"));
            Assert.That(addedFeedback.Type, Is.EqualTo("ChargingStation"));
        }

        [Test]
        public async Task UpdateFeedbackStatusAsync_UpdatesStatusSuccessfully()
        {
            var result = await _feedbackService.ResolveFeedback(2, "Processed", "Thank you for your feedback!");

            var updatedFeedback = await _context.Feedbacks.FindAsync(2);
            Assert.That(updatedFeedback!.Status, Is.EqualTo("Processed"));
            Assert.That(updatedFeedback.Response, Is.EqualTo("Thank you for your feedback!"));
        }

        [Test]
        public async Task UpdateFeedbackStatusAsync_FeedbackNotFound_ReturnsFalse()
        {
            var resultNotFound = await _feedbackService.ResolveFeedback(999, "Processed", "Response");

            Assert.That(resultNotFound, Is.EqualTo(false));
        }

    }


}
