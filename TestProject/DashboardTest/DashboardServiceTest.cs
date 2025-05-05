using API.Services;
using DataAccess.DTOs.ChargingStation;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.DashboardTest
{
    public class DashboardServiceTest
    {
        private WccsContext _context;
        private DashboardService _dashboardService;
        private DashboardRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

            _context = new WccsContext(options);
            _repository = new DashboardRepository(_context);
            _dashboardService = new DashboardService(_repository);
            SeedTestData(_context);
        }

        private void SeedTestData(WccsContext context)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);

            var carModel = new CarModel
            {
                CarModelId = 1,
            };

            var user = new User
            {
                UserId = 1,
                RoleId = 1,
                CreateAt = today.AddDays(-5),
                Email = "test@example.com",
                PhoneNumber = "123"
            };

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
                User = user,
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
                User = user,
                StartTime = startOfWeek.AddDays(1),
                EndTime = startOfWeek.AddDays(1).AddMinutes(45),
                EnergyConsumed = 15,
                Cost = 30000
            };

            context.Users.Add(user);
            context.CarModels.Add(carModel);
            context.Cars.Add(car);
            context.ChargingStations.AddRange(station, offlineStation);
            context.ChargingPoints.Add(chargingPoint);
            context.ChargingSessions.AddRange(session1, session2);

            context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }
        [Test]
        public async Task GetSystemOverviewAsync_ShouldReturnCorrectValues()
        {
            // Act
            var result = await _dashboardService.GetSystemOverviewAsync();

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

        //[Test]
        //public async Task GetSessionStatistics_ReturnsCorrectStatistics()
        //{
        //    // Arrange
        //    var filter = new FilterDto
        //    {
        //        Start = DateTime.Today.AddDays(-7), // test with a date range
        //        End = DateTime.Today.AddDays(7)
        //    };

        //    // Act
        //    var result = await _dashboardService.GetSessionStatistics(filter);

        //    // Assert
        //    Assert.That(result.TotalSession, Is.EqualTo(2));
        //    Assert.That(result.TotalEnergy, Is.EqualTo(35));
        //    Assert.That(result.AvgDuration, Is.EqualTo(37.5)); 
        //    Assert.That(result.FailedSessions, Is.EqualTo(0)); 
        //}

        //[Test]
        //public async Task GetRevenueStatistics_ReturnsCorrectRevenue()
        //{
        //    // Arrange
        //    var filter = new FilterDto
        //    {
        //        Start = DateTime.Today.AddDays(-7), // test with a date range
        //        End = DateTime.Today.AddDays(7)
        //    };

        //    // Act
        //    var result = await _dashboardService.GetRevenueStatistics(filter);

        //    // Assert
        //    Assert.That(result.TotalRevenue, Is.EqualTo(80000));
        //    Assert.That(result.AvgRevenuePerSession, Is.EqualTo(40000));
        //    Assert.That(result.Daily.Count, Is.EqualTo(1));
        //    Assert.That(result.ByStation.Count, Is.EqualTo(1));
        //}

        [Test]
        public async Task GetUserStatistics_ReturnsCorrectUserStatistics()
        {
            // Arrange
            var filter = new FilterDto
            {
                Start = DateTime.Today.AddDays(-7), // test with a date range
                End = DateTime.Today.AddDays(7)
            };

            // Act
            var result = _dashboardService.GetUserStatistics(filter);

            // Assert
            Assert.That(result.TotalUsers, Is.EqualTo(1)); 
            Assert.That(result.UsersWhoCharged, Is.EqualTo(1)); 
            Assert.That(result.UsersWhoNeverCharged, Is.EqualTo(0)); 
        }
    }
}
