using DataAccess.DTOs.ChargingStation;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TestProject.DashboardTest
{
    public class DashboardRepoTest
    {
        private WccsContext _context;
        private DashboardRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WccsContext(options);
            _repository = new DashboardRepository(_context);
            SeedTestData(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedTestData(WccsContext context)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);

            var carModel = new CarModel
            {
                CarModelId = 1,
            };

            var user2 = new User { UserId = 1, RoleId = 1, CreateAt = today.AddDays(-10), Email = "user1@example.com", PhoneNumber = "123" };
            var user3 = new User { UserId = 2, RoleId = 1, CreateAt = today.AddDays(-5), Email = "user2@example.com", PhoneNumber = "123" };
            var user4 = new User { UserId = 3, RoleId = 1, CreateAt = today.AddDays(-2), Email = "user3@example.com", PhoneNumber = "123" };
            var user5 = new User { UserId = 4, RoleId = 1, CreateAt = today.AddDays(-1), Email = "user4@example.com", PhoneNumber = "123" };
            
            var station = new ChargingStation
            {
                StationId = 1,
                Status = "active",
                OwnerId = 1,
                StationLocationId = 1
            };

            var offlineStation = new ChargingStation
            {
                StationId = 2,
                Status = "offline",
                OwnerId = 1,
                StationLocationId = 2
            };

            var chargingPoint = new ChargingPoint
            {
                ChargingPointId = 1,
                StationId = 1,
                Station = station
            };

            var car = new Car
            {
                CarId = 1,
                CarModelId = 1,
                CarModel = carModel
            };

            var session1 = new ChargingSession
            {
                SessionId = 1,
                CarId = 1,
                Car = car,
                ChargingPointId = 1,
                ChargingPoint = chargingPoint,
                UserId = 1,
                User = user2,
                StartTime = today,
                EndTime = today.AddMinutes(30),
                EnergyConsumed = 20,
                Cost = 50000
            };

            var session2 = new ChargingSession
            {
                SessionId = 2,
                CarId = 1,
                Car = car,
                ChargingPointId = 1,
                ChargingPoint = chargingPoint,
                UserId = 1,
                User = user2,
                StartTime = startOfWeek.AddDays(1),
                EndTime = startOfWeek.AddDays(1).AddMinutes(45),
                EnergyConsumed = 15,
                Cost = 30000
            };

            context.Users.Add(user2);
            context.Users.Add(user3);
            context.Users.Add(user4);
            context.Users.Add(user5);
            context.CarModels.Add(carModel);
            context.Cars.Add(car);
            context.ChargingStations.AddRange(station, offlineStation);
            context.ChargingPoints.Add(chargingPoint);
            context.ChargingSessions.AddRange(session1, session2);

            context.SaveChanges();
        }


        [Test]
        public async Task GetSystemOverviewAsync_ShouldReturnCorrectValues()
        {
            // Act
            var result = await _repository.GetSystemOverviewAsync();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.TotalStations, Is.EqualTo(2));
                Assert.That(result.TotalChargingPoints, Is.EqualTo(1));
                Assert.That(result.TodaySessions, Is.EqualTo(2));
                Assert.That(result.WeekSessions, Is.EqualTo(2));
                Assert.That(result.TotalEnergyToday, Is.EqualTo(35));
                Assert.That(result.TotalEnergyThisMonth, Is.EqualTo(35));
                Assert.That(result.TotalRevenue, Is.EqualTo(80000));
                Assert.That(result.ActiveStations, Is.EqualTo(1));
                Assert.That(result.OfflineStations, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task GetSessionsAsync_NoFilter_ReturnsAllSessions()
        {
            var filter = new FilterDto();
            var result = await _repository.GetSessionsAsync(filter);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetSessionsAsync_FilterByStationId_ReturnsCorrectSessions()
        {
            var filter = new FilterDto { StationId = 1 };
            var result = await _repository.GetSessionsAsync(filter);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(s => s.ChargingPoint!.StationId == 1), Is.True);
        }

        [Test]
        public async Task GetSessionsAsync_FilterByDateRange_ReturnsCorrectSessions()
        {
            var filter = new FilterDto
            {
                Start = DateTime.Today.AddDays(-1),
                End = DateTime.Today.AddDays(1)
            };
            var result = await _repository.GetSessionsAsync(filter);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetStatistics_NoFilter_ReturnsCorrectStatistics()
        {
            // Arrange
            var filter = new FilterDto();

            // Act
            var result = _repository.GetStatistics(filter);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.TotalUsers, Is.EqualTo(4)); // tổng 4 user
                Assert.That(result.UsersWhoCharged, Is.EqualTo(1)); // 2 user có phiên sạc
                Assert.That(result.UsersWhoNeverCharged, Is.EqualTo(3)); // 2 user chưa sạc
                Assert.That(result.NewUsersOverTime.Count, Is.GreaterThanOrEqualTo(1)); // group theo ngày CreateAt
                Assert.That(result.MAU, Is.GreaterThanOrEqualTo(1)); // tháng này có user sạc
                Assert.That(result.DAU, Is.GreaterThanOrEqualTo(1)); // hôm nay có user sạc
                Assert.That(result.DAUMAUPercentage, Is.GreaterThanOrEqualTo(0)); // tỷ lệ hợp lệ
            });
        }

        [Test]
        public void GetStatistics_FilterByStartDate_ReturnsFilteredStatistics()
        {
            // Arrange
            var filter = new FilterDto
            {
                Start = DateTime.Today.AddDays(-3) // lọc user tạo từ 3 ngày trước đến nay
            };

            // Act
            var result = _repository.GetStatistics(filter);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.TotalUsers, Is.EqualTo(4)); // Total không thay đổi
                Assert.That(result.NewUsersOverTime.All(x => x.Date >= filter.Start!.Value.Date), Is.True); // NewUser từ start trở đi
            });
        }

        [Test]
        public void GetStatistics_NoUserInDateRange_ReturnsZeroNewUsers()
        {
            // Arrange
            var filter = new FilterDto
            {
                Start = DateTime.Today.AddYears(-2),
                End = DateTime.Today.AddYears(-2).AddDays(1)
            };

            // Act
            var result = _repository.GetStatistics(filter);

            // Assert
            Assert.That(result.NewUsersOverTime.Count, Is.EqualTo(0));
        }

    }
}
