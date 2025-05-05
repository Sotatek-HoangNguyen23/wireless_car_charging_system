using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

namespace TestProject.ChargingStationTest
{
    public class ChargingStationRepoTest
    {
        private const decimal latitude = 10;
        private const decimal longitude = 10;
        private IChargingStationRepository _repository;
        private WccsContext _context;

        [SetUp]
        public void Setup()
        {
            // Tạo in‑memory database với tên ngẫu nhiên để có dữ liệu sạch cho mỗi test.
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new WccsContext(options);

            // Seed dữ liệu Users
            var user1 = new User
            {
                UserId = 1,
                Fullname = "Nguyen Van A",
                Email = "a@example.com",
                PhoneNumber = "0123456789",
                CreateAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                UserId = 2,
                Fullname = "Tran Thi B",
                Email = "b@example.com",
                PhoneNumber = "0987654321",
                CreateAt = DateTime.UtcNow
            };

            // Seed dữ liệu StationLocation
            var location1 = new StationLocation
            {
                StationLocationId = 1,
                Address = "123 ABC Street, HCM City",
                Latitude = 10.762622M,
                Longitude = 106.660172M
            };

            // Seed dữ liệu ChargingStations
            // Station 1: ID 2016 sẽ chứa các phiên sạc (charging sessions)
            var station1 = new ChargingStation
            {
                StationId = 2016,
                OwnerId = 1,
                StationLocationId = 1,
                StationName = "Hanoi Station",
                Status = "Available",
                MaxConsumPower = 50,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                StationLocation = location1,
                Owner = user1,
                ChargingPoints = new List<ChargingPoint>()
            };

            // Tạo điểm sạc cho station1
            var cp1 = new ChargingPoint
            {
                ChargingPointId = 1,
                Status = "Available"
                // Giả sử rằng ChargingPoint có quan hệ với ChargingStation qua thuộc tính StationId
                // Nếu có, bạn có thể gán: StationId = station1.StationId;
            };
            station1.ChargingPoints.Add(cp1);

            // Station 2: ID 2021 với nhiều điểm sạc và dùng cho các test khác
            var station2 = new ChargingStation
            {
                StationId = 2021,
                OwnerId = 1,
                StationLocationId = 1,
                StationName = "Ho Chi Minh Station",
                Status = "Available",
                MaxConsumPower = 55,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                StationLocation = location1,
                Owner = user1,
                ChargingPoints = new List<ChargingPoint>
                {
                    new ChargingPoint
                    {
                        ChargingPointId = 2,
                        Status = "Available"
                    },
                    new ChargingPoint
                    {
                        ChargingPointId = 3,
                        Status = "In Use"
                    },
                    new ChargingPoint
                    {
                        ChargingPointId = 4,
                        Status = "Available"
                    }
                }
            };

            // Station 3 & Station 4 cho kiểm tra phân trang
            var station3 = new ChargingStation
            {
                StationId = 2022,
                OwnerId = 2,
                StationLocationId = 1,
                StationName = "Da Nang Station",
                Status = "Available",
                MaxConsumPower = 60,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                StationLocation = location1,
                Owner = user2,
                ChargingPoints = new List<ChargingPoint>()
            };

            var station4 = new ChargingStation
            {
                StationId = 2023,
                OwnerId = 2,
                StationLocationId = 1,
                StationName = "Can Tho Station",
                Status = "Available",
                MaxConsumPower = 65,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                StationLocation = location1,
                Owner = user2,
                ChargingPoints = new List<ChargingPoint>()
            };

            // Seed dữ liệu ChargingSessions
            // Ở đây, thay vì gán StationId, ta gán ChargingPointId để liên kết phiên sạc với station1 thông qua cp1.
            var session1 = new ChargingSession
            {
                SessionId = 1,
                CarId = 1, // Giả sử có Car với id là 1
                ChargingPointId = cp1.ChargingPointId,
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
                ChargingPointId = cp1.ChargingPointId,
                UserId = user1.UserId,
                StartTime = DateTime.UtcNow.AddMinutes(-60),
                EndTime = DateTime.UtcNow.AddMinutes(-30),
                EnergyConsumed = 6.0,
                Cost = 12,
                Status = "Completed"
            };

            // Thêm các entity vào context
            _context.Users.AddRange(user1, user2);
            _context.StationLocations.Add(location1);
            _context.ChargingStations.AddRange(station1, station2, station3, station4);
            _context.ChargingSessions.AddRange(session1, session2);

            // Lưu lại dữ liệu seed
            _context.SaveChanges();

            // Khởi tạo repository
            _repository = new ChargingStationRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }


        // Test GetAllStation no filter
        //[Test]
        //public void GetAllStation_ShouldReturnAllStations_WhenNoFiltersApplied()
        //{
        //    var result = _repository.GetAllStation(null, null, null, 1, 10);

        //    Assert.That(result.Data, Has.Count.EqualTo(4));
        //    Assert.That(result.TotalPages, Is.EqualTo(1));
        //}

        //// Test GetAllStation with keyword
        //[Test]
        //public void GetAllStation_ShouldReturnFilteredStations_WhenKeywordMatches()
        //{
        //    var result = _repository.GetAllStation("Ho Chi", null, null, 1, 10);

        //    Assert.That(result.Data, Has.Count.EqualTo(1));
        //    Assert.That(result.Data.First().StationName, Is.EqualTo("Ho Chi Minh Station"));
        //}

        //// Test GetAllStation with my location
        //[Test]
        //public void GetAllStation_ShouldReturnNearestStations_WhenUserLocationProvided()
        //{
        //    var result = _repository.GetAllStation(null, (decimal?)10.762622, (decimal?)106.660172, 1, 10);

        //    Assert.That(result.Data, Has.Count.EqualTo(4));
        //    Assert.That(result.Data.First().StationName, Is.EqualTo("Hanoi Station"));
        //}

        //// Test GetAllStation paginated
        //[Test]
        //public void GetAllStation_ShouldPaginateResults_Correctly()
        //{
        //    var result = _repository.GetAllStation(null, null, null, 2, 2);

        //    Assert.That(result.Data, Has.Count.EqualTo(2));
        //    Assert.That(result.TotalPages, Is.EqualTo(2));
        //}

        // Test GetStationById when Id not exist
        [Test]
        public void GetStationById_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var result = _repository.GetStationById(999);

            Assert.That(result, Is.Null);
        }

        // Test GetStationById when Id  exist
        [Test]
        public void GetStationById_ShouldReturnCorrectStation_WhenIdExists()
        {
            var result = _repository.GetStationById(2021);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StationId, Is.EqualTo(2021));
            Assert.That(result.StationName, Is.EqualTo("Ho Chi Minh Station"));
            Assert.That(result.Owner, Is.EqualTo("Nguyen Van A"));
            Assert.That(result.Address, Is.EqualTo("123 ABC Street, HCM City"));
        }

        [Test]
        public void GetStationById_ShouldReturnCorrectChargingPointsCount2()
        {
            var result = _repository.GetStationById(2021);

            Assert.That(result, Is.Not.Null);
            // station2 có 3 điểm sạc và trong đó có 2 điểm có trạng thái "Available"
            Assert.That(result.TotalPoint, Is.EqualTo(3));
            Assert.That(result.AvailablePoint, Is.EqualTo(2));
        }

        // Test AddChargingStation
        [Test]
        public async Task AddChargingStation_ShouldAddNewStation()
        {
            var newStation = new ChargingStation
            {
                StationName = "Station Test",
                OwnerId = 2,
                Status = "Available",
                MaxConsumPower = 60,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                StationLocation = new StationLocation
                {
                    Address = "Test Address",
                    Latitude = latitude,
                    Longitude = longitude
                },
                ChargingPoints = new List<ChargingPoint>()
            };

            var result = await _repository.AddChargingStation(newStation);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StationId, Is.EqualTo(2024));
            Assert.That(result.StationName, Is.EqualTo("Station Test"));
            Assert.That(result.Status, Is.EqualTo("Available"));
            Assert.That(result.Owner.Fullname, Is.EqualTo("Tran Thi B"));

            var dbStation = await _context.ChargingStations.FindAsync(2024);
            Assert.That(dbStation, Is.Not.Null);
        }

        [Test]
        public async Task AddChargingStation_ShouldThrowCustomException_WhenOwnerNotFound()
        {
            // Arrange
            var station = new ChargingStation
            {
                StationName = "New Station",
                OwnerId = 9999, // Owner không tồn tại
                Status = "Available",
                MaxConsumPower = 60,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                StationLocation = new StationLocation
                {
                    Address = "Test Address",
                    Latitude = latitude,
                    Longitude = longitude
                }
            };

            // Act
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _repository.AddChargingStation(station);
            });

            // Assert
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Owner does not exist."));
        }


        // Test UpdateChargingStation when Id exist
        [Test]
        public async Task UpdateChargingStation_ShouldUpdateStation_WhenIdExists()
        {
            var updateDto = new UpdateChargingStationDto
            {
                StationName = "Updated Station",
                Status = "In Used",
                MaxConsumPower = 70
            };

            var result = await _repository.UpdateChargingStation(2021, updateDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StationName, Is.EqualTo("Updated Station"));
            Assert.That(result.Status, Is.EqualTo("In Used"));
            Assert.That(result.MaxConsumPower, Is.EqualTo(70.0));
        }

        // Test UpdateChargingStation when Id not exist
        [Test]
        public async Task UpdateChargingStation_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var updateDto = new UpdateChargingStationDto
            {
                StationName = "Nonexistent Station"
            };

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _repository.UpdateChargingStation(999, updateDto);
            });

            Assert.That(ex.Message, Is.EqualTo("Station does not exist."));
        }

        // Test DeleteChargingStation When Station Id exist
        [Test]
        public async Task DeleteChargingStation_ShouldReturnTrue_WhenStationExists()
        {
            var stationId = 2023;

            var result = await _repository.DeleteChargingStation(stationId);
            var updatedStation = _repository.GetStationById(stationId);

            Assert.That(result, Is.Not.Null);
            Assert.That(updatedStation, Is.Not.Null);
            Assert.That(updatedStation.Status, Is.EqualTo("Deleted"));
        }


        // Test DeleteChargingStation When Station Id not exist
        [Test]
        public async Task DeleteChargingStation_ShouldReturnFalse_WhenStationDoesNotExist()
        {
            // Arrange
            var stationId = 9999; // stationId không tồn tại

            // Act
            var result = await _repository.DeleteChargingStation(stationId);

            // Assert
            Assert.That(result, Is.Null);
        }


        [Test]
        public void GetSessionByStation_ShouldReturnSessions_WhenStationExists()
        {
            var result = _repository.GetSessionByStation(2016);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].EnergyConsumed, Is.EqualTo(5.5));
        }

        [Test]
        public void GetSessionByStation_ShouldThrowException_WhenStationIdIsNull()
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                _repository.GetSessionByStation(999);
            });

            Assert.That(ex.Message, Is.EqualTo("Station does not exist."));
        }

        [Test]
        public async Task AddStationLocation_ShouldSaveAndReturnLocation()
        {
            // Arrange
            var location = new StationLocation
            {
                Address = "123 ABC Street",
                Latitude = 10.123m,
                Longitude = 106.456m,
                Description = "Near park",
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            // Act
            var result = await _repository.AddStationLocation(location);

            // Assert
            Assert.That(result.StationLocationId, Is.EqualTo(2)); // Được gán ID sau khi Save
            Assert.That(_context.StationLocations.CountAsync().Result, Is.EqualTo(2));
            Assert.That(result.Address, Is.EqualTo("123 ABC Street"));
        }
    }
}