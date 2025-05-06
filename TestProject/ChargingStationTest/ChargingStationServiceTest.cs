using API.Services;
using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;

namespace TestProject.ChargingStationTest
{
    [TestFixture]
    public class ChargingStationServiceTest
    {
        private WccsContext _context;
        private ChargingStationService _service;
        private IChargingStationRepository _stationRepository;
        private IChargingPointRepository _pointRepository;
        private const decimal latitude = 10;
        private const decimal longitude = 10;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new WccsContext(options);

            // Seed dữ liệu
            var user1 = new User { UserId = 1, Fullname = "Nguyen Van A", Email = "a@example.com", PhoneNumber = "0123456789", CreateAt = 
                Now };
            var user2 = new User { UserId = 2, Fullname = "Tran Thi B", Email = "b@example.com", PhoneNumber = "0987654321", CreateAt = DateTime.UtcNow };

            var location1 = new StationLocation
            {
                StationLocationId = 1,
                Address = "123 ABC Street, HCM City",
                Latitude = 10.762622M,
                Longitude = 106.660172M
            };

            var station1 = new ChargingStation
            {
                StationId = 1,
                OwnerId = 1,
                StationLocationId = 1,
                StationName = "Hanoi Station",
                Status = "Available",
                MaxConsumPower = 50,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            var station2 = new ChargingStation
            {
                StationId = 2,
                OwnerId = 1,
                StationLocationId = 1,
                StationName = "HCM Station",
                Status = "Available",
                MaxConsumPower = 60,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            var points = new List<ChargingPoint>
            {
                new ChargingPoint { ChargingPointId = 1, ChargingPointName = "Point 1", Status = "Available", MaxPower = 50, StationId = 1 },
                new ChargingPoint { ChargingPointId = 2, ChargingPointName = "Point 2", Status = "In Use", MaxPower = 50, StationId = 2 },
                new ChargingPoint { ChargingPointId = 3, ChargingPointName = "Point 3", Status = "Available", MaxPower = 60, StationId = 2 }
            };

            var session1 = new ChargingSession
            {
                SessionId = 1,
                CarId = 1, // Giả sử có Car với id là 1
                ChargingPointId = 1,
                UserId = user1.UserId,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                EndTime = DateTime.UtcNow,
                EnergyConsumed = 5.5,
                Cost = 10,
                Status = "Completed"
            };

            var session2 = new ChargingSession
            {
                SessionId = 2,
                CarId = 1,
                ChargingPointId = 1,
                UserId = user1.UserId,
                StartTime = DateTime.UtcNow.AddMinutes(-60),
                EndTime = DateTime.UtcNow.AddMinutes(-30),
                EnergyConsumed = 6.0,
                Cost = 12,
                Status = "Completed"
            };

            _context.Users.AddRange(user1, user2);
            _context.StationLocations.Add(location1);
            _context.ChargingStations.AddRange(station1, station2);
            _context.ChargingPoints.AddRange(points);
            _context.ChargingSessions.AddRange(session1, session2);
            _context.SaveChanges();

            _stationRepository = new ChargingStationRepository(_context);
            _pointRepository = new ChargingPointRepository(_context);

            _service = new ChargingStationService(_stationRepository, _pointRepository);
            // Add mock data
            var stationLocations = new List<StationLocation>
            {
                new StationLocation
                {
                    StationLocationId = 1,
                    Address = "123 ABC Street",
                    Latitude = latitude,
                    Longitude = longitude,
                    Description = "Near the park",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                },
                new StationLocation
                {
                    StationLocationId = 2,
                    Address = "456 XYZ Street",
                    Latitude = latitude,
                    Longitude = longitude,
                    Description = "Near the mall",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        //[Test]
        //public void GetChargingStations_ShouldReturnAllStations_NoFilter()
        //{
        //    var result = _service.GetChargingStations(null, 0, 0, 1, 10);

        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.Data.Count, Is.EqualTo(2));
        //}

        [Test]
        public async Task GetStationDetails_ShouldReturnStationWithPoints_WhenIdNotExists()
        {
            var result = _service.GetStationDetails(999, 1, 10);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetStationDetails_ShouldReturnStationWithPoints_WhenIdExists()
        {
            var result = _service.GetStationDetails(2, 1, 10);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Station.StationName, Is.EqualTo("HCM Station"));
            Assert.That(result.Points.Data.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task AddChargingStation_ShouldAddStation_Successfully()
        {
            var newStation = new NewChargingStationDto
            {
                StationName = "New Station",
                OwnerId = 1,
                Address = "New Address",
                Latitude = 11,
                Longitude = 106,
                LocationDescription = "Near mall",
                TotalPoint = 2,
                PointCode = "NS",
                PointDescription = "Fast charger",
                MaxPower = 100
            };

            var result = await _service.AddChargingStation(newStation);

            Assert.That(result, Is.True);
            Assert.That(_context.ChargingStations.Count(), Is.EqualTo(3));
        }
        [Test]
        public async Task UpdateChargingStation_ShouldUpdateStation_WhenIdExists()
        {
            // Ensure the station exists before updating
            var existingStation = _context.ChargingStations.Find(1);
            Assert.That(existingStation, Is.Not.Null, "The station with ID 1 should exist.");

            var updateDto = new UpdateChargingStationDto
            {
                StationName = "Updated Name",
                Status = "Inactive"
            };

            var result = await _service.UpdateChargingStation(1, updateDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StationName, Is.EqualTo("Updated Name"));
            Assert.That(result.Status, Is.EqualTo("Inactive"));
        }

        [Test]
        public async Task UpdateChargingStation_ShouldReturnNull_NonExistingStation()
        {
            var updateDto = new UpdateChargingStationDto
            {
                StationName = "Updated Name",
                Status = "Inactive"
            };

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        await _service.UpdateChargingStation(999, updateDto));

            Assert.That(ex.Message, Is.EqualTo("Station not found!"));
        }

        [Test]
        public async Task DeleteChargingStation_ShouldReturnFalse_WhenIdNotExist()
        {
            var result = await _service.DeleteChargingStation(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteChargingStation_ShouldDeleteStation_WhenIdExists()
        {
            var result = await _service.DeleteChargingStation(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo("Deleted"));
        }

        // Test GetPointById
        [Test]
        public void GetPointById_ShouldReturnNull_WhenIdNotExist()
        {
            var result = _service.GetPointById(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetPointById_ShouldReturnCorrectPoint_WhenIdExists()
        {
            var point = _service.GetPointById(1);

            Assert.That(point, Is.Not.Null);
            Assert.That(point.ChargingPointName, Is.EqualTo("Point 1"));
        }

        // Test AddPoint
        [Test]
        public async Task AddPoint_ShouldAddCorrectNumberOfPoints_WhenStationIdExist()
        {
            var stationDto = new NewChargingStationDto
            {
                TotalPoint = 2,
                PointCode = "PX",
                PointDescription = "Test charger",
                MaxPower = 80
            };

            var points = await _service.AddPoint(1, stationDto);

            Assert.That(points.Count, Is.EqualTo(2));
            Assert.That(points[0].ChargingPointName, Does.StartWith("PX"));
        }

        [Test]
        public async Task AddPoint_ShouldReturnEmpty_NonExistingStation()
        {
            var stationDto = new NewChargingStationDto
            {
                TotalPoint = 2,
                PointCode = "PX",
                PointDescription = "Test charger",
                MaxPower = 80
            };

            var points = await _service.AddPoint(999, stationDto);

            Assert.That(points, Is.Empty);
        }

        [Test]
        public async Task UpdateChargingPoint_ShouldUpdatePoint_WhenIdNotExists()
        {
            var updateDto = new UpdateChargingPointDto
            {
                ChargingPointName = "Updated Point",
                Status = "In Use"
            };

            var result = await _service.UpdateChargingPoint(999, updateDto);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateChargingPoint_ShouldUpdatePoint_WhenIdExists()
        {
            var updateDto = new UpdateChargingPointDto
            {
                ChargingPointName = "Updated Point",
                Status = "In Use"
            };

            var result = await _service.UpdateChargingPoint(1, updateDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ChargingPointName, Is.EqualTo("Updated Point"));
            Assert.That(result.Status, Is.EqualTo("In Use"));
        }

        [Test]
        public async Task DeleteChargingPoint_ShouldReturnFalse_WhenIdNotExist()
        {
            var result = await _service.DeleteChargingPoint(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteChargingPoint_ShouldDeletePoint_WhenIdExists()
        {
            var result = await _service.DeleteChargingPoint(2);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo("Deleted"));
        }

        [Test]
        public void GetStats_ShouldReturnCorrectStats_WhenDataExists()
        {
            var stationId = 1;
            var year = 2025;
            var month = 4;

            var result = _service.GetStats(stationId, year, month);


            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalEnergyConsumed, Is.EqualTo(11.5));
            Assert.That(result.TotalRevenue, Is.EqualTo(22.0));
            Assert.That(result.TotalChargingSessions, Is.EqualTo(2));
            Assert.That(result.AverageChargingTime, Is.EqualTo(30));

            Assert.That(result.ChartData.Count, Is.EqualTo(1));
            Assert.That(result.ChartData[0].Label, Is.EqualTo("Ngày 29"));
        }

        [Test]
        public void GetStats_ShouldReturnEmptyStats_WhenNoSessionsMatchFilter()
        {
            var stationId = 1;
            var year = 2023;
            var month = 1;

            var result = _service.GetStats(stationId, year, month);

            Assert.That(result.TotalEnergyConsumed, Is.EqualTo(0));
            Assert.That(result.TotalRevenue, Is.EqualTo(0));
            Assert.That(result.TotalChargingSessions, Is.EqualTo(0));
            Assert.That(result.AverageChargingTime, Is.EqualTo(0));
            Assert.That(result.ChartData, Is.Empty);
        }
    }
}
