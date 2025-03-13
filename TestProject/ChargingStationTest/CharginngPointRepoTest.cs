using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;

namespace TestProject.ChargingStationTest
{ 
    public class CharginngPointRepoTest
    {
        private IChargingPointRepository _repository;
        private WccsContext _context = new();

        [SetUp]
        public void Setup()
        {
            _repository = new ChargingPointRepository(_context);
        }

        // Test GetAllPointsByStation
        [Test]
        public void GetAllPointsByStation_ShouldReturnPagedResult()
        {
            var result = _repository.GetAllPointsByStation(2, page: 1, pageSize: 10);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data.Count, Is.EqualTo(3)); 
            Assert.That(result.TotalPages, Is.EqualTo(1));
        }

        // Test GetPointById when Id exist
        [Test]
        public void GetPointById_ShouldReturnPoint_WhenIdExists()
        {
            var result = _repository.GetPointById(4);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ChargingPointId, Is.EqualTo(4));
            Assert.That(result.ChargingPointName, Is.EqualTo("HCM-1"));
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
        public async Task AddChargingPoints_ShouldAddPoints()
        {
            var newPoints = new List<ChargingPoint>
            {
                new ChargingPoint
                {
                    ChargingPointName = "Point Test",
                    Description = "Slow Charger",
                    Status = "Busy",
                    MaxPower = 50,
                    MaxConsumPower = 40,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                    StationId = 2
                }
            };

            await _repository.AddChargingPoints(newPoints);
            var result = _repository.GetPointById(2042);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ChargingPointName, Is.EqualTo("Point Test"));
        }

        // Test GetPointById when Id exist
        [Test]
        public void AddChargingPoints_ShouldThrowException_WhenEmptyList()
        {
            var emptyList = new List<ChargingPoint>();

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _repository.AddChargingPoints(emptyList));

            Assert.That(ex.Message, Is.EqualTo("Point list is empty!"));
        }

        // Test UpdateChargingPoint when Id exist
        [Test]
        public async Task UpdateChargingPoint_ShouldUpdatePoint_WhenIdExists()
        {
            var updateDto = new UpdateChargingPointDto
            {
                ChargingPointName = "Updated Point",
                Status = "Unavailable",
                MaxPower = 120
            };

            var result = await _repository.UpdateChargingPoint(2042, updateDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ChargingPointName, Is.EqualTo("Updated Point"));
            Assert.That(result.Status, Is.EqualTo("Unavailable"));
            Assert.That(result.MaxPower, Is.EqualTo(120));
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
        public async Task DeleteChargingPoint_ShouldDelete_WhenIdExists()
        {
            var result = await _repository.DeleteChargingPoint(2040);
            var deletedPoint = _repository.GetPointById(2040);

            Assert.That(result, Is.True);
            Assert.That(deletedPoint, Is.Null);
        }

        // Test DeleteChargingPoint when Id not exist
        [Test]
        public async Task DeleteChargingPoint_ShouldReturnFalse_WhenIdNotExists()
        {
            var result = await _repository.DeleteChargingPoint(999);

            Assert.That(result, Is.False);
        }

    }
}
