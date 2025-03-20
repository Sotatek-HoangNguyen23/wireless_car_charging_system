using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;

namespace TestProject.ChargingStationTest
{
    public class ChargingStationRepoTest
    {
        private const decimal latitude = 10;
        private const decimal longitude = 10;
        private IChargingStationRepository _repository;
        private WccsContext _context = new();

        [SetUp]
        public void Setup()
        {
            _repository = new ChargingStationRepository(_context);
        }

        // Test GetAllStation no filter
        [Test]
        public void GetAllStation_ShouldReturnAllStations_WhenNoFiltersApplied()
        {
            var result = _repository.GetAllStation(null, null, null, 1, 10);

            Assert.That(result.Data, Has.Count.EqualTo(4));
            Assert.That(result.TotalPages, Is.EqualTo(1));
        }

        // Test GetAllStation with keyword
        [Test]
        public void GetAllStation_ShouldReturnFilteredStations_WhenKeywordMatches()
        {
            var result = _repository.GetAllStation("Ho Chi", null, null, 1, 10);

            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data.First().StationName, Is.EqualTo("Ho Chi Minh Station"));
        }

        // Test GetAllStation with my location
        [Test]
        public void GetAllStation_ShouldReturnNearestStations_WhenUserLocationProvided()
        {
            var result = _repository.GetAllStation(null, (decimal?)10.762622, (decimal?)106.660172, 1, 10);

            Assert.That(result.Data, Has.Count.EqualTo(4));
            Assert.That(result.Data.First().StationName, Is.EqualTo("Ho Chi Minh Station"));
        }

        // Test GetAllStation paginated
        [Test]
        public void GetAllStation_ShouldPaginateResults_Correctly()
        {
            var result = _repository.GetAllStation(null, null, null, 2, 2);

            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.TotalPages, Is.EqualTo(2));
        }

        // Test GetStationById when Id exist
        [Test]
        public void GetStationById_ShouldReturnCorrectStation_WhenIdExists()
        {
            var result = _repository.GetStationById(2);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StationId, Is.EqualTo(2));
            Assert.That(result.StationName, Is.EqualTo("Ho Chi Minh Station"));
            Assert.That(result.Owner, Is.EqualTo("Nguyen Van A"));
            Assert.That(result.Address, Is.EqualTo("123 ABC Street, HCM City"));
        }

        // Test GetStationById when Id not exist
        [Test]
        public void GetStationById_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var result = _repository.GetStationById(999);

            Assert.That(result, Is.Null);
        }

        // Test GetStationById have point
        [Test]
        public void GetStationById_ShouldReturnCorrectChargingPointsCount()
        {
            var result = _repository.GetStationById(2);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalPoint, Is.EqualTo(3)); // 2 điểm sạc
            Assert.That(result.AvailablePoint, Is.EqualTo(2)); // 1 điểm "Available"
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
            Assert.That(result.StationId, Is.EqualTo(2023));
            Assert.That(result.StationName, Is.EqualTo("Station Test"));
            Assert.That(result.Status, Is.EqualTo("Available"));
            Assert.That(result.Owner.Fullname, Is.EqualTo("Tran Thi B"));

            var dbStation = await _context.ChargingStations.FindAsync(2023);
            Assert.That(dbStation, Is.Not.Null);
        }

        // Test UpdateChargingStation when Id exist
        [Test]
        public async Task UpdateChargingStation_ShouldUpdateStation_WhenIdExists()
        {
            var updateDto = new UpdateChargingStationDto
            {
                StationName = "Updated Station",
                OwnerId = 1,
                Status = "In Used",
                MaxConsumPower = 70,
                Latitude = 100,
                Longtitude = 100,
                Address = "New Address"
            };

            var result = await _repository.UpdateChargingStation(2021, updateDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StationName, Is.EqualTo("Updated Station"));
            Assert.That(result.Owner.Fullname, Is.EqualTo("Nguyen Van A"));
            Assert.That(result.Status, Is.EqualTo("In Used"));
            Assert.That(result.MaxConsumPower, Is.EqualTo(70.0));
            Assert.That(result.StationLocation, Is.Not.Null);
            Assert.That(result.StationLocation.Latitude, Is.EqualTo(100));
            Assert.That(result.StationLocation.Longitude, Is.EqualTo(100));
            Assert.That(result.StationLocation.Address, Is.EqualTo("New Address"));
        }

        // Test UpdateChargingStation when Id not exist
        [Test]
        public async Task UpdateChargingStation_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var updateDto = new UpdateChargingStationDto
            {
                StationName = "Nonexistent Station"
            };

            var result = await _repository.UpdateChargingStation(999, updateDto);
            Assert.That(result, Is.Null);
        }

        // Test DeleteChargingStation when Id exist
        [Test]
        public async Task DeleteChargingStation_ShouldDeleteStation_WhenIdExists()
        {
            var result = await _repository.DeleteChargingStation(2016);

            Assert.That(result, Is.True);
            var deletedStation = await _context.ChargingStations.FindAsync(2016);
            Assert.That(deletedStation, Is.Null);
        }

        // Test DeleteChargingStation when Id not exist
        [Test]
        public async Task DeleteChargingStation_ShouldReturnFalse_WhenIdDoesNotExist()
        {
            var result = await _repository.DeleteChargingStation(999);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetSessionByStation_ShouldReturnSessions_WhenStationExists()
        {
            var result = _repository.GetSessionByStation(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].EnergyConsumed, Is.EqualTo(5.5));
            Assert.That(result[1].EnergyConsumed, Is.EqualTo(6.0));
        }

        [Test]
        public void GetSessionByStation_ShouldReturnEmpty_WhenStationHasNoSessions()
        {
            var result = _repository.GetSessionByStation(999);

            Assert.That(result, Is.Empty);
        }
    }
}