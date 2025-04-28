using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Testing.Platform.Extensions.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.UserTest
{
    [TestFixture]
    public class UserRepoTest
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
                Fullname= "Nguyen Van A",
                PhoneNumber = "0123456789",
                Dob=DateTime.UtcNow,
                Gender= true,
                Address="address test",
                CreateAt = DateTime.UtcNow,
                Cccds = new List<Cccd>
                {
                    new Cccd
                    {
                        Code = "123456789",
                        ImgFront = "front.jpg",
                        ImgBack = "back.jpg"
                    }
                }
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

        //Tim Profile theo UserId
        // Tim cccd o user khong co cccd => tra ve null
        [Test]
        public async Task GetProfileByUserId_NoCccd_ReturnsNullCccdFields()
        {
            // Arrange
            var userWithoutCccd = new User { UserId = 2, Email = "nocc@test.com", PhoneNumber = "0999999999" };
            await _context.Users.AddAsync(userWithoutCccd);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProfileByUserId(2);

            // Assert
            Assert.That(result!.CccdId, Is.EqualTo(0));
            Assert.That(result.ImgFront, Is.Null);
        }
        // Tim user co cccd => tra ve profile co cccd
        [Test]
        public async Task GetProfileByUserId_ValidId_ReturnsProfileWithCccd()
        {
            // Act
            var result = await _repository.GetProfileByUserId(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.CccdId, Is.GreaterThan(0));
            Assert.That(result.ImgFront, Is.EqualTo("front.jpg"));
        }
        // UserId khong ton tai => tra ve null
        [Test]
        public async Task GetProfileByUserId_InvalidId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetProfileByUserId(999);

            // Assert
            Assert.That(result, Is.Null);
        }
        // Tim user theo Cccd
        // cccd khong ton tai => tra ve null
        [Test]
        public async Task GetUserByCccd_InvalidCode_ReturnsNull()
        {
            // Act
            var result = await _repository.GetUserByCccd("invalid_cccd");

            // Assert
            Assert.That(result, Is.Null);
        }
        //Tim user theo cccd
        // cccd hop le => tra ve user
        [Test]
        public async Task GetUserByCccd_ValidCode_ReturnsUser()
        {
            // Act
            var result = await _repository.GetUserByCccd("123456789");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Cccds.First().Code, Is.EqualTo("123456789"));
        }

        [Test]
        public void GetUserByCccd_EmptyCode_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetUserByCccd(""));
            Assert.That(ex.Message, Does.Contain("CCCD không thể trống"));
        }
        [Test]
        public void GetUserByCccd_WhiteSpaceCode_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetUserByCccd("   "));
            Assert.That(ex.Message, Does.Contain("CCCD không thể trống"));
        }
        // tim user theo phone
        //Phone co khoang trang => tra ve exception
        [Test]
        public void GetUserByPhone_WhiteSpace_ThrowsException()
        {
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>_repository.GetUserByPhone("   "));
            Assert.That(ex.Message, Does.Contain("không thể trống"));
        }
        // phone khong ton tai => tra ve null
        [Test]
        public async Task GetUserByPhone_InvalidPhone_ReturnsNull()
        {
            // Act
            var result = await _repository.GetUserByPhone("invalid_phone");
            // Assert
            Assert.That(result, Is.Null);
        }
        // phone rong => tra ve exception
        [Test]
        public void GetUserByPhone_EmptyPhone_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetUserByPhone(""));
            Assert.That(ex.Message, Does.Contain("Số điện thoại không thể trống"));
        }
        // phone hop le => tra ve user
        [Test]
        public async Task GetUserByPhone_ValidPhone_ReturnsUser()
        {
            // Act
            var result = await _repository.GetUserByPhone("0123456789");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.PhoneNumber, Is.EqualTo("0123456789"));
        }

        //Tim user theo email
        // Email hop le => tra ve user
        [Test]
        public async Task GetUserByEmail_ValidEmail_ReturnsUser()
        {
            // Act
            var result = await _repository.GetUserByEmail("test@example.com");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Email, Is.EqualTo("test@example.com"));
        }
        // Email khong ton tai => tra ve null
        [Test]
        public async Task GetUserByEmail_InvalidEmail_ReturnNull()
        {
            // Act
            var result = await _repository.GetUserByEmail("invalid_email");
            // Assert
            Assert.That(result, Is.Null);
        }
        // Email rong => tra ve exception
        [Test]
        public void GetUserByEmail_EmptyEmail_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetUserByEmail(""));

            Assert.That(ex.Message, Does.Contain("Email không thể trống"));
        }   
        [Test]
        public void GetUserByEmail_WhiteSpace_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetUserByEmail("      "));

            Assert.That(ex.Message, Does.Contain("Email không thể trống"));
        }
        // them user
        [Test]
        public void SaveUser_NullUser_ThrowsException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _repository.SaveUser(null!));
        }
        [Test]
        public async Task SaveUser_ValidUser_AddsToDatabase()
        {
            // Arrange
            var newUser = new User
            {
                UserId = 2,
                Email = "new@example.com",
                PhoneNumber = "0987654321",
                RoleId = 2
            };

            // Act
            await _repository.SaveUser(newUser);

            // Assert
            var users = await _context.Users.ToListAsync();
            Assert.That(users, Has.Count.EqualTo(2));
        }
        [Test]
        public async Task GetUserById_ValidId_ReturnsUserWithRole()
        {
            // Act
            var result = await _repository.GetUserById(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Role.RoleName, Is.EqualTo("Driver"));
        }



        [Test]
        public async Task UpdateUser_ValidData_UpdatesSuccessfully()
        {
            // Arrange
            var user = await _repository.GetUserById(1);
            user!.Email = "updated@example.com";

            // Act
            await _repository.UpdateUser(user);

            // Assert
            var updatedUser = await _repository.GetUserById(1);
            Assert.That(updatedUser!.Email, Is.EqualTo("updated@example.com"));
        }
        [Test]
        public async Task UpdateUser_MultipleFields_UpdatesAllProperties()
        {
            // Arrange
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == 1);

            if (user != null)
            {
                user.PhoneNumber = "0987654321";
                user.Address = "New Address";

                // Act
                await _repository.UpdateUser(user);

                // Assert
                var updated = await _repository.GetUserById(1);
                Assert.Multiple(() =>
                {
                    Assert.That(updated!.PhoneNumber, Is.EqualTo("0987654321"));
                    Assert.That(updated.Address, Is.EqualTo("New Address"));
                });
            }
            else
            {
                Assert.Fail("User not found");
            }
        }

        // doi trang thai user
        [Test]
        public async Task ChangeUserStatus_ValidRequest_UpdatesStatus()
        {
            // Act
            await _repository.ChangeUserStatusAsync(1, "Inactive");

            // Assert
            var user = await _repository.GetUserById(1);
            Assert.That(user!.Status, Is.EqualTo("Inactive"));
        }
        [Test]
        public async Task ChangeUserStatus_InvalidUser_NoExceptionThrown()
        {
            // Act
            await _repository.ChangeUserStatusAsync(999, "Inactive");

            // Assert
            Assert.DoesNotThrowAsync(() => _repository.ChangeUserStatusAsync(999, "Inactive"));
        }
        [Test]
        public void ChangeUserStatus_EmptyStatus_UpdatesToNull()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _repository.ChangeUserStatusAsync(1, ""));
            Assert.That(ex.Message, Does.Contain("Trạng thái không thể trống"));
        }
        //----------------------
        [Test]
        public async Task GetUserByEmailOrPhone_ValidEmail_ReturnsUser()
        {
            // Arrange
            var search = "test@example.com";

            // Act
            var result = await _repository.GetUserByEmailOrPhone(search);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Email, Is.EqualTo("test@example.com"));
        }

        [Test]
        public async Task GetUserByEmailOrPhone_ValidPhone_ReturnsUser()
        {
            // Arrange
            var search = "0123456789";

            // Act
            var result = await _repository.GetUserByEmailOrPhone(search);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].PhoneNumber, Is.EqualTo("0123456789"));
        }

        [Test]
        public async Task GetUserByEmailOrPhone_PartialMatch_ReturnsUser()
        {
            // Arrange
            var search = "test"; // Tìm kiếm một phần email

            // Act
            var result = await _repository.GetUserByEmailOrPhone(search);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Email, Does.Contain(search));
        }

        [Test]
        public async Task GetUserByEmailOrPhone_NoMatch_ReturnsEmptyList()
        {
            // Arrange
            var search = "nonexistent";

            // Act
            var result = await _repository.GetUserByEmailOrPhone(search);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetUserByEmailOrPhone_EmptyString_ReturnsAllUsers()
        {
            // Arrange
            var search = "";
            await _context.Users.AddAsync(new User { UserId = 2, Email = "user2@test.com", PhoneNumber = "0987654321" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserByEmailOrPhone(search);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        public void GetUserByEmailOrPhone_NullSearch_ThrowsArgumentNullException()
        {
            // Arrange
            string search = null!;

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.GetUserByEmailOrPhone(search));

            Assert.That(ex.ParamName, Is.EqualTo("Tìm kiếm không thể trống hoặc khoảng trắng"));
        }

        [Test]
        public async Task GetUserByEmailOrPhone_CaseSensitive_ReturnsNoResults()
        {
            // Arrange
            var search = "TEST@EXAMPLE.COM"; // Chữ hoa

            // Act
            var result = await _repository.GetUserByEmailOrPhone(search);

            // Assert
            Assert.That(result, Is.Empty); // Không tìm thấy do case-sensitive
        }

        [Test]
        public async Task GetUserByEmailOrPhone_MultipleMatches_ReturnsAll()
        {
            // Arrange
            var search = "test";
            await _context.Users.AddAsync(new User
            {
                UserId = 2,
                Email = "another_test@test.com",
                PhoneNumber = "01234test567"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserByEmailOrPhone(search);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2)); // Cả 2 user đều có "test" trong email hoặc phone
        }

        private async Task addUser()
        {
            var users = new List<User>
        {
            new User
            {
                UserId = 2,
                RoleId = 1,
                Fullname = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "123456789",
                Status = "Active"
            },
            new User
            {
                UserId = 3,
                RoleId = 2,
                Fullname = "Jane Doe",
                Email = "jane@example.com",
                PhoneNumber = "987654321",
                Status = "Inactive"
            },
            new User
            {
                UserId = 4,
                RoleId = 1,
                Fullname = "Bob Smith",
                Email = "bob@example.com",
                PhoneNumber = "555555555",
                Status = "Pending"
            }
        };
            _context.Users.AddRange(users);
           await _context.SaveChangesAsync();
        }
        private async Task addFeedback()
        {
            var feedbacks = new List<Feedback>
        {
            new Feedback
            {
                FeedbackId = 1,
                UserId = 1,
                Message = "Great service",
                CreatedAt = new DateTime(2023, 1, 1)
            },
            new Feedback
            {
                FeedbackId = 2,
                UserId = 1,
                Message = "Need improvement",
                CreatedAt = new DateTime(2023, 2, 1)
            },
            new Feedback
            {
                FeedbackId = 3,
                UserId = 2,
                Message = "Urgent fix needed",
                CreatedAt = new DateTime(2023, 3, 1)
            },
            new Feedback
            {
                FeedbackId = 4,
                UserId = 2,
                Message = "Good support",
                CreatedAt = new DateTime(2023, 4, 1)
            } 
            };
            await _context.Feedbacks.AddRangeAsync(feedbacks);
             await _context.SaveChangesAsync();
        }
        [Test]
        public async Task GetUsers_FilterByRoleId_ReturnsFilteredUsers()
        {
            await addUser();
            // Arrange
            int roleId = 1;

            // Act
            var result = _repository.GetUsers(
                searchQuery: null,
                status: null,
                roleId: roleId,
                pageNumber: 1,
                pageSize: 10
            );

            // Assert
            Assert.That(result.Data, Has.Count.EqualTo(3));
            Assert.That(result.Data.All(u => u.Role.RoleId == 1), Is.True);
        }

        [Test]
        public async Task GetUsers_SearchQuery_ReturnsMatchingUsers()
        {
            await addUser();
            // Arrange
            string searchQuery = "Doe";

            // Act
            var result = _repository.GetUsers(
                searchQuery,
                status: null,
                roleId: null,
                pageNumber: 1,
                pageSize: 10
            );  

            // Assert
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.Data.Any(u => u.UserId == 1), Is.False);
            Assert.That(result.Data.Any(u => u.UserId == 2), Is.True);
        }

        [Test]
        public async Task GetUsers_FilterByStatus_ReturnsCorrectUsers()
        {
            await addUser();

            // Arrange
            string status = "Active";

            // Act
            var result = _repository.GetUsers(
                searchQuery: null,
                status,
                roleId: null,
                pageNumber: 1,
                pageSize: 10
            );

            // Assert
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data[0].Status, Is.EqualTo("Active"));
        }

        [Test]
        public async Task GetUsers_MultipleFilters_CombinesCorrectly()
        {
            await addUser();

            // Arrange
            int roleId = 1;
            string searchQuery = "Bob";

            // Act
            var result = _repository.GetUsers(
                searchQuery,
                status: null,
                roleId,
                pageNumber: 1,
                pageSize: 10
            );

            // Assert
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data[0].UserId, Is.EqualTo(4));
        }

        [Test]
        public async Task GetUsers_Pagination_ReturnsCorrectPage()
        {
            await addUser();

            // Arrange
            int pageNumber = 2;
            int pageSize = 2;

            // Act
            var result = _repository.GetUsers(
                searchQuery: null,
                status: null,
                roleId: null,
                pageNumber,
                pageSize
            );

            // Assert
            Assert.That(result.Data, Has.Count.EqualTo(2)); // Trang 2 có 2 item
            Assert.That(result.TotalPages, Is.EqualTo(2)); // Tổng 4 items → 2 trang
        }

        [Test]
        public async Task GetUsers_InvalidPageNumber_ReturnsFirstPage()
        {
            await addUser();

            // Arrange
            int pageNumber = -1; // Giá trị không hợp lệ
            int pageSize = 2;

            // Act
            var result = _repository.GetUsers(
                searchQuery: null,
                status: null,
                roleId: null,
                pageNumber,
                pageSize
            );

            // Assert
            Assert.That(result.Data, Has.Count.EqualTo(2)); // Trả về trang đầu
        }

        //Feedbacks Tests
        [Test]
        public async Task GetFeedbacks_NoFilters_ReturnsAllFeedbacks()
        {
            await addUser();
            await addFeedback();
            // Arrange
            int pageSize = 10;

            // Act
            var result = _repository.GetFeedbacks(
                search: null,
                startDate: null,
                endDate: null,
                page: 1,
                pageSize: pageSize
            );

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Data, Has.Count.EqualTo(4));
                Assert.That(result.TotalPages, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task GetFeedbacks_SearchByEmail_ReturnsMatching()
        {
            await addUser();
            await addFeedback();
            // Act
            var result = _repository.GetFeedbacks(
                search: "test@example.com",
                startDate: null,
                endDate: null,
                page: 1,
                pageSize: 10
            );

            // Assert
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.Data.All(f => f.User == "test@example.com"));
        }

        [Test]
        public async Task GetFeedbacks_DateFilter_ReturnsCorrectRange()
        {
            await addUser();
            await addFeedback();
            // Arrange
            var startDate = new DateTime(2023, 3, 1);
            var endDate = new DateTime(2023, 4, 1);

            // Act
            var result = _repository.GetFeedbacks(
                search: null,
                startDate: startDate,
                endDate: endDate,
                page: 1,
                pageSize: 10
            );

            // Assert
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.Data.Any(f => f.Id == 3));
            Assert.That(result.Data.Any(f => f.Id == 4));
        }

        [Test]
        public async Task GetFeedbacks_Pagination_ReturnsCorrectPage()
        {
            await addUser();
            await addFeedback();
            // Act
            var result = _repository.GetFeedbacks(
                search: null,
                startDate: null,
                endDate: null,
                page: 2,
                pageSize: 2
            );

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Data, Has.Count.EqualTo(2));
                Assert.That(result.Data.First().Id, Is.EqualTo(2));
                Assert.That(result.TotalPages, Is.EqualTo(2));
            });
        }

        // GetFeedbackByUserId Tests
        [Test]
        public async Task GetFeedbackByUserId_ValidId_ReturnsOrderedFeedbacks()
        {
            await addUser();
            await addFeedback();
            // Act
            var result = await _repository.GetFeedbackByUserId(1);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result[0].FeedbackId, Is.EqualTo(2)); // Latest first
                Assert.That(result[1].FeedbackId, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task GetFeedbackByUserId_InvalidId_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetFeedbackByUserId(999);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetFeedbackByUserId_NoFeedbacks_ReturnsEmptyList()
        {
            // Arrange
            var feedbacksToDelete = await _context.Feedbacks
                .Where(f => f.UserId == 2)
                .ToListAsync();

            _context.Feedbacks.RemoveRange(feedbacksToDelete);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFeedbackByUserId(2);

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}