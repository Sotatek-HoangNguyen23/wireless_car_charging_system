using API.Services;
using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;

namespace TestProject.ChargingStationTest
{
    public class ChargingStationServiceTest
    {
        private WccsContext _context = new();
        private ChargingStationService _service;
        private IChargingStationRepository _stationRepository;
        private IChargingPointRepository _pointRepository;
        private IChargingLocationRepository _locationRepository;

        private const decimal latitude = 10;
        private const decimal longitude = 10;

        [SetUp]
        public void Setup()
        {
            _stationRepository = new ChargingStationRepository(_context);
            _pointRepository = new ChargingPointRepository(_context);
            _locationRepository = new StationLocationRepository(_context);
            _service = new ChargingStationService(_stationRepository, _pointRepository, _locationRepository);
        }

        [Test]
        public void GetChargingStations_ShouldReturnEmpty_WhenNoStationsExist()
        {
            var result = _service.GetChargingStations(null, 0, 0, 1, 10);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetChargingStations_ShouldReturnStations_WhenDataExists()
        {
            var result = _service.GetChargingStations(null, 0, 0, 1, 20);

            Assert.That(result.Data.Count, Is.EqualTo(11));
            Assert.That(result.Data[0].StationName, Is.EqualTo("Station Test"));
        }

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
            Assert.That(result.Station.StationName, Is.EqualTo("Ho Chi Minh Station"));
            Assert.That(result.Points.Data.Count, Is.EqualTo(5));
            Assert.That(result.Points.Data[0].ChargingPointName, Is.EqualTo("HCM-1"));
        }

        [Test]
        public async Task AddChargingStation_ShouldAddStation_Successfully()
        {
            var newStation = new NewChargingStationDto
            {
                StationName = "Test Station",
                OwnerId = 2,
                Address = "456 XYZ Street",
                Latitude = latitude,
                Longtitude = longitude,
                LocationDescription = "Near the mall",
                TotalPoint = 5,
                PointCode = "P",
                PointDescription = "Fast charger",
                MaxPower = 100
            };

            var result = await _service.AddChargingStation(newStation);

            Assert.That(result, Is.True);
            Assert.That(_context.ChargingStations.Count(), Is.EqualTo(12));
        }

        [Test]
        public async Task UpdateChargingStation_ShouldUpdateStation_WhenIdExists()
        {
            var updateDto = new UpdateChargingStationDto
            {
                StationName = "New Name",
                Status = "Inactive"
            };

            var result = await _service.UpdateChargingStation(2024, updateDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StationName, Is.EqualTo("New Name"));
            Assert.That(result.Status, Is.EqualTo("Inactive"));
        }

        [Test]
        public async Task DeleteChargingStation_ShouldReturnFalse_WhenIdNotExist()
        {
            var result = await _service.DeleteChargingStation(999);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteChargingStation_ShouldDeleteStation_WhenIdExists()
        {
            var result = await _service.DeleteChargingStation(2017);

            Assert.That(result, Is.True);
            Assert.That(_context.ChargingStations.Count(), Is.EqualTo(11));
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
            var result = _service.GetPointById(4);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ChargingPointName, Is.EqualTo("HCM-1"));
        }

        // Test AddPoint
        [Test]
        public async Task AddPoint_ShouldAddCorrectNumberOfPoints_WhenStationIdExist()
        {
            var stationDto = new NewChargingStationDto
            {
                TotalPoint = 3,
                PointCode = "P",
                PointDescription = "Fast charger",
                MaxPower = 100
            };

            // Act
            var result = await _service.AddPoint(2, stationDto);

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[^3].ChargingPointName, Is.EqualTo("P-7"));
            Assert.That(result[^2].ChargingPointName, Is.EqualTo("P-8"));
            Assert.That(result[^1].ChargingPointName, Is.EqualTo("P-9"));
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

            var result = await _service.UpdateChargingPoint(2047, updateDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ChargingPointName, Is.EqualTo("Updated Point"));
            Assert.That(result.Status, Is.EqualTo("In Use"));
        }

        [Test]
        public async Task DeleteChargingPoint_ShouldReturnFalse_WhenIdNotExist()
        {
            var result = await _service.DeleteChargingPoint(999);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteChargingPoint_ShouldDeletePoint_WhenIdExists()
        {
            var result = await _service.DeleteChargingPoint(2046);

            Assert.That(result, Is.True);
            Assert.That(_context.ChargingPoints.Count(), Is.EqualTo(18));
        }

        [Test]
        public void GetStats_ShouldReturnCorrectStats_WhenDataExists()
        {
            var result = _service.GetStats(1, 2025, 3);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalEnergyConsumed, Is.EqualTo(11.5));
            Assert.That(result.TotalRevenue, Is.EqualTo(22.0));
            Assert.That(result.TotalChargingSessions, Is.EqualTo(2));
            Assert.That(result.AverageChargingTime, Is.EqualTo(37.5));

            Assert.That(result.ChartData.Count, Is.EqualTo(2));
            Assert.That(result.ChartData[0].Label, Is.EqualTo("Ngày 1"));
            Assert.That(result.ChartData[1].Label, Is.EqualTo("Ngày 2"));
        }

        [Test]
        public void GetStats_ShouldReturnEmptyStats_WhenNoSessionsMatchFilter()
        {
            var result = _service.GetStats(1, 2023, 3);

            Assert.That(result.TotalEnergyConsumed, Is.EqualTo(0));
            Assert.That(result.TotalRevenue, Is.EqualTo(0));
            Assert.That(result.TotalChargingSessions, Is.EqualTo(0));
            Assert.That(result.AverageChargingTime, Is.EqualTo(0));
            Assert.That(result.ChartData, Is.Empty);
        }
    }
}
