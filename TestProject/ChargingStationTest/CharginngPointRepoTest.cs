using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;

namespace TestProject.ChargingStationTest
{ 
    public class CharginngPointRepoTest
    {
        private IChargingPointRepository _repository;
        private WccsContext context = new();

        [SetUp]
        public void Setup()
        {
            _repository = new ChargingPointRepository(context);
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
                    ChargingPointId = 2,
                    ChargingPointName = "Point B",
                    Description = "Slow Charger",
                    Status = "Busy",
                    MaxPower = 50,
                    MaxConsumPower = 40,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                    StationId = 101
                }
            };

            await _repository.AddChargingPoints(newPoints);
            var result = _repository.GetPointById(2);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ChargingPointName, Is.EqualTo("Point B"));
        }

        [Test]
        public void AddChargingPoints_ShouldThrowException_WhenEmptyList()
        {
            // Arrange
            var emptyList = new List<ChargingPoint>();

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _repository.AddChargingPoints(emptyList));

            Assert.That(ex.Message, Is.EqualTo("Point list is empty!"));
        }

    }
}
