using DataAccess.DTOs.UserDTO;
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
    public class LicenseRepoTest
    {
        private WccsContext _context;
        private DriverLicenseRepository _repository;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WccsContext(options);
            _repository = new DriverLicenseRepository(_context);

            var users = new List<User>
            {
                new User
            {
                UserId = 1,
                Fullname = "Nguyen Van A",
                Email = "a@test.com",
                PhoneNumber = "1111111111",
                CreateAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 2,
                Fullname = "Tran Thi B",
                Email = "b@test.com",
                PhoneNumber = "2222222222",
                CreateAt = DateTime.UtcNow
            }
            };
            var licenses = new List<DriverLicense>
            {
            new DriverLicense
            {
                DriverLicenseId = 1,
                Code = "LICENSE001",
                Class = "B2",
                Status = "Active",
                CreateAt = new DateTime(2023, 1, 1),
                UpdateAt = new DateTime(2023, 1, 5),
                UserId = 1,
                ImgFront = "front1.jpg",
                ImgBack = "back1.jpg"
            },
            new DriverLicense
            {
                DriverLicenseId = 2,
                Code = "LICENSE002",
                Class = "C",
                Status = "InActive",
                CreateAt = new DateTime(2023, 2, 1),
                UpdateAt = new DateTime(2023, 2, 5),
                UserId = 1, 
                ImgFront = "front2.jpg",
                ImgBack = "back2.jpg"
            },
            new DriverLicense
            {
                DriverLicenseId = 3,
                Code = "LICENSE003",
                Class = "B1",
                Status = "InActive",
                CreateAt = new DateTime(2023, 3, 1),
                UpdateAt = new DateTime(2023, 3, 5),
                UserId = 2, 
                ImgFront = "front3.jpg",
                ImgBack = "back3.jpg"
            }
            };


            await _context.Users.AddRangeAsync(users);
            await _context.DriverLicenses.AddRangeAsync(licenses);
            await _context.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
        [Test]
        public async Task GetPagedLicensesAsync_NoFilter_ReturnsAllRecords()
        {
            // Arrange
            var filter = new DriverLicenseFilter();

            // Act
            var result = await _repository.GetPagedLicensesAsync(1, 10, filter);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Items, Has.Count.EqualTo(3));
                Assert.That(result.TotalPages, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task GetPagedLicensesAsync_FilterByStatus_ReturnsCorrectRecords()
        {
            // Arrange
            var filter = new DriverLicenseFilter { Status = "Active" };

            // Act
            var result = await _repository.GetPagedLicensesAsync(1, 10, filter);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Items, Has.Count.EqualTo(1));
                Assert.That(result.Items[0].LicenseNumber, Is.EqualTo("LICENSE001"));
            });
        } 
        [Test]
        public async Task GetPagedLicensesAsync_FilterByStatus2_ReturnsCorrectRecords()
        {
            // Arrange
            var filter = new DriverLicenseFilter { Status = "InActive" };

            // Act
            var result = await _repository.GetPagedLicensesAsync(1, 10, filter);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Items, Has.Count.EqualTo(2));
                Assert.That(result.Items[0].LicenseNumber, Is.EqualTo("LICENSE003"));
            });
        }
        [Test]
        public async Task GetPagedLicensesAsync_FilterByFullname_ReturnsCorrectRecords()
        {
            // Arrange
            var filter = new DriverLicenseFilter { Fullname = "Nguyen Van A" };

            // Act
            var result = await _repository.GetPagedLicensesAsync(1, 10, filter);

            // Assert
            Assert.That(result.Items, Has.Count.EqualTo(2));
            Assert.That(result.Items.All(dl => dl.User.Fullname.Contains("Nguyen Van A")));
        }

        [Test]
        public async Task GetPagedLicensesAsync_FilterByDateRange_ReturnsCorrectRecords()
        {
            // Arrange
            var filter = new DriverLicenseFilter
            {
                FromCreateDate = new DateTime(2023, 2, 1),
                ToCreateDate = new DateTime(2023, 3, 1)
            };

            // Act
            var result = await _repository.GetPagedLicensesAsync(1, 10, filter);

            // Assert
            Assert.That(result.Items, Has.Count.EqualTo(2));
        }
        [Test]
        public async Task GetPagedLicensesAsync_Page2_ReturnsCorrectRecords()
        {
            // Act
            var result = await _repository.GetPagedLicensesAsync(2, 2, new DriverLicenseFilter());

            // Assert
            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].LicenseNumber, Is.EqualTo("LICENSE001"));
        }
        [Test]
        public async Task DeleteLicense_ValidId_RemovesFromDatabase()
        {
            // Act
            await _repository.DeleteLicense("1");

            // Assert
            var license = await _context.DriverLicenses.FindAsync(1);
            Assert.That(license, Is.Null);
        }
        [Test]
        public void DeleteLicense_InvalidFormatId_ThrowsFormatException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.DeleteLicense("invalid_id"));
            Assert.That(ex.Message, Does.Contain("LicenseId phải là số nguyên"));
        }

        // Thêm các test case mới
        [Test]
        public void DeleteLicense_EmptyId_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.DeleteLicense(""));
            Assert.That(ex.ParamName, Is.EqualTo("licenseId"));
            Assert.That(ex.Message, Does.Contain("không hợp lệ"));
        }

        [Test]
        public void DeleteLicense_WhiteSpaceId_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.DeleteLicense("   "));
            Assert.That(ex.ParamName, Is.EqualTo("licenseId"));
            Assert.That(ex.Message, Does.Contain("không hợp lệ"));
        }

        [Test]
        public void DeleteLicense_NonExistentId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.DeleteLicense("999"));
            Assert.That(ex.Message, Does.Contain("Không tìm thấy"));
        }

        [Test]
        public async Task DeleteLicense_ValidId_RemovesRecord()
        {
            // Act
            await _repository.DeleteLicense("1");

            // Assert
            var license = await _context.DriverLicenses.FindAsync(1);
            Assert.That(license, Is.Null);
        }

        [Test]
        public async Task GetLicenseByCode_ExistingCode_ReturnsLicense()
        {
            // Act
            var result = await _repository.GetLicenseByCode("LICENSE001");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Code, Is.EqualTo("LICENSE001"));
            });
        }

        [Test]
        public async Task SaveLicense_NewLicense_AddsToDatabase()
        {
            // Arrange
            var newLicense = new DriverLicense
            {
                Code = "NEW123",
                Class = "D",
                Status = "Pending",
                UserId = 1
            };

            // Act
            await _repository.SaveLicense(newLicense);

            // Assert
            var savedLicense = await _context.DriverLicenses.FirstOrDefaultAsync(l => l.Code == "NEW123");
            Assert.That(savedLicense, Is.Not.Null);
        }
        [Test]
        public void SaveLicense_DuplicateCode_ThrowsDbUpdateException()
        {
            // Arrange
            var license = new DriverLicense { Code = "LICENSE001", UserId = 1 };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.SaveLicense(license));
            Assert.That(_context.DriverLicenses.Count(l => l.Code == "LICENSE001"), Is.EqualTo(1));
        }
        [Test]
        public async Task UpdateLicense_ExistingLicense_UpdatesProperties()
        {
            // Arrange
            var license = await _context.DriverLicenses.FindAsync(1);
            license.Status = "Suspended";

            // Act
            await _repository.UpdateLicense(license);

            // Assert
            var updatedLicense = await _context.DriverLicenses.FindAsync(1);
            Assert.That(updatedLicense.Status, Is.EqualTo("Suspended"));
        }
        [Test]
        public async Task UpdateLicense_UpdatesMultipleFields_Correctly()
        {
            // Arrange
            var license = await _context.DriverLicenses.FindAsync(1);
            license.Class = "NewClass";
            license.Code = "UPDATED123";

            // Act
            await _repository.UpdateLicense(license);

            // Assert
            var updated = await _context.DriverLicenses.FindAsync(1);
            Assert.Multiple(() =>
            {
                Assert.That(updated.Class, Is.EqualTo("NewClass"));
                Assert.That(updated.Code, Is.EqualTo("UPDATED123"));
            });
        }
        [Test]
        public async Task GetLicensesByUserId_ValidUserId_ReturnsMatchingRecords()
        {
            // Act
            var result = await _repository.GetLicensesByUserId(1);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result.All(l => l.UserId == 1));
            });
        }
        [Test]
        public async Task GetLicensesByUserId_InvalidUserId_ReturnsEmpty()
        {
            var result = await _repository.GetLicensesByUserId(999);
            Assert.That(result, Is.Empty);
        }
    }
}
