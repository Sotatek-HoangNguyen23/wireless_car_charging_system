using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;

namespace TestProject.ChargingStationTest
{ 
    public class CharginngPointRepoTest
    {
        private IChargingPointRepository _repository;
        private WccsContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WccsContext(options);

            // Create a user (required because ChargingStation has an OwnerId foreign key)
            var user = new User
            {
                UserId = 1,
                Fullname = "Test User",
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                CreateAt = DateTime.UtcNow
            };

            // Create a station location
            var location = new StationLocation
            {
                StationLocationId = 1,
                Address = "Test Address",
                Latitude = 10.762622M,
                Longitude = 106.660172M
            };

            // Create a charging station
            var station = new ChargingStation
            {
                StationId = 1,
                StationName = "Test Station",
                OwnerId = 1,
                StationLocationId = 1,
                Status = "Available",
                MaxConsumPower = 50,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                Owner = user,
                StationLocation = location,
                ChargingPoints = new List<ChargingPoint>()
            };

            // Create charging points
            var chargingPoints = new List<ChargingPoint>
            {
                new ChargingPoint
                {
                    ChargingPointId = 1,
                    ChargingPointName = "Point 1",
                    Status = "Available",
                    MaxPower = 50,
                    MaxConsumPower = 40,
                    StationId = 1,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                },
                new ChargingPoint
                {
                    ChargingPointId = 2,
                    ChargingPointName = "Point 2",
                    Status = "In Use",
                    MaxPower = 60,
                    MaxConsumPower = 50,
                    StationId = 1,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                }
            };

            _context.ChargingPoints.AddRange(chargingPoints);

            // Add seed data
            _context.Users.Add(user);
            _context.StationLocations.Add(location);
            _context.ChargingStations.Add(station);
            _context.SaveChanges();

            _repository = new ChargingPointRepository(_context);
            // Add initial data
            var station = new ChargingStation
            {
                StationId = 2,
                StationName = "Test Station",
                Status = "Active",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                ChargingPoints = new List<ChargingPoint>
                {
                    new ChargingPoint
                    {
                        ChargingPointId = 4,
                        ChargingPointName = "HCM-1",
                        Description = "Fast charger",
                        Status = "Available",
                        MaxPower = 100,
                        MaxConsumPower = 90,
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow,
                        StationId = 2
                    },
                    new ChargingPoint
                    {
                        ChargingPointId = 5,
                        ChargingPointName = "HCM-2",
                        Description = "Fast charger",
                        Status = "Available",
                        MaxPower = 100,
                        MaxConsumPower = 90,
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow,
                        StationId = 2
                    },
                    new ChargingPoint
                    {
                        ChargingPointId = 6,
                        ChargingPointName = "HCM-3",
                        Description = "Fast charger",
                        Status = "Available",
                        MaxPower = 100,
                        MaxConsumPower = 90,
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow,
                        StationId = 2
                    }
                }
            };

            _context.ChargingStations.Add(station);
            _context.SaveChanges();
        }
        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        // Test GetAllPointsByStation
        [Test]
        public void GetAllPointsByStation_ShouldReturnPoints()
        {
            var result = _repository.GetAllPointsByStation(1, page: 1, pageSize: 10);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetAllPointsByStation_ShouldReturnNull_WhenStationIdNotExist()
        {
            var result = _repository.GetAllPointsByStation(999, page: 1, pageSize: 10);

            Assert.That(result.Data.Count, Is.EqualTo(0));
        }

        // Test GetPointById when Id exist
        [Test]
        public void GetPointById_ShouldReturnPoint_WhenIdExists()
        {
            var result = _repository.GetPointById(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ChargingPointId, Is.EqualTo(1));
            Assert.That(result.ChargingPointName, Is.EqualTo("Point 1"));
        }

        // Test GetPointById when Id exist
        [Test]
        public void GetPointById_ShouldReturnNull_WhenIdNotExists()
        {
            var result = _repository.GetPointById(999); // ID không tồn tại

            Assert.That(result, Is.Null);
        }

        // Test AddChargingPoints
        [Test]
        public async Task AddChargingPoints_ShouldAddNewPoint()
        {
            var newPoints = new List<ChargingPoint>
            {
                new ChargingPoint
                {
                    ChargingPointName = "New Point",
                    Status = "Available",
                    MaxPower = 70,
                    MaxConsumPower = 60,
                    StationId = 1,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                }
            };

            await _repository.AddChargingPoints(newPoints);

            var addedPoint = _context.ChargingPoints.FirstOrDefault(x => x.ChargingPointName == "New Point");

            Assert.That(addedPoint, Is.Not.Null);
            Assert.That(addedPoint.MaxPower, Is.EqualTo(70));
        }

        // Test GetPointById when Id exist
        [Test]
        public void AddChargingPoints_ShouldThrowException_WhenListIsEmpty()
        {
            var emptyList = new List<ChargingPoint>();

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _repository.AddChargingPoints(emptyList));

            Assert.That(ex.Message, Is.EqualTo("Point list is empty!"));
        }

        // Test UpdateChargingPoint when Id exist
        [Test]
        public async Task UpdateChargingPoint_ShouldUpdate_WhenIdExists()
        {
            var updateDto = new UpdateChargingPointDto
            {
                ChargingPointName = "Updated Name",
                Status = "Unavailable",
                MaxPower = 80
            };

            var updatedPoint = await _repository.UpdateChargingPoint(1, updateDto);

            Assert.That(updatedPoint, Is.Not.Null);
            Assert.That(updatedPoint.ChargingPointName, Is.EqualTo("Updated Name"));
            Assert.That(updatedPoint.Status, Is.EqualTo("Unavailable"));
        }


        // Test UpdateChargingPoint when Id not exist
        [Test]
        public async Task UpdateChargingPoint_ShouldReturnNull_WhenIdNotExists()
        {
            var updateDto = new UpdateChargingPointDto
            {
                ChargingPointName = "Non-existing Point"
            };

            var result = await _repository.UpdateChargingPoint(999, updateDto);

            Assert.That(result, Is.Null);
        }

        // Test DeleteChargingPoint when Id exist
        [Test]
        public async Task DeleteChargingPoint_ShouldSetStatusToRemoved_WhenIdExists()
        {
            // Arrange
            var pointId = 2; // ChargingPointId có sẵn từ dữ liệu seed

            // Act
            var result = await _repository.DeleteChargingPoint(pointId);
            var updatedPoint = _repository.GetPointById(pointId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(updatedPoint, Is.Not.Null);
            Assert.That(updatedPoint.Status, Is.EqualTo("Deleted"));
        }


        // Test DeleteChargingPoint when Id not exist
        [Test]
        public async Task DeleteChargingPoint_ShouldReturnNull_WhenIdNotExists()
        {
            // Act
            var result = await _repository.DeleteChargingPoint(999); // Id không tồn tại

            // Assert
            Assert.That(result, Is.Null);
        }


    }
}
