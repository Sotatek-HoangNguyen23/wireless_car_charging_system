using API.Services;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.CarRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.MyCarTest
{
    [TestFixture]
    public class CarServiceTest
    {
        private WccsContext _context;
        private MyCarsRepo _myCarsRepo;
        private CarService _carService;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WccsContext(options);
            _myCarsRepo = new MyCarsRepo(_context);
            _carService = new CarService(_myCarsRepo);

            // Set up base test data
            await SetupTestData();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        private async Task SetupTestData()
        {
            // Create station location
            var stationLocation = new StationLocation
            {
                StationLocationId = 1,
                Address = "123 Test Street",
                Latitude = 10.5m,
                Longitude = 106.7m,
                Description = "Test Location",
                CreateAt = DateTime.Now
            };

            // Create owner user
            var owner = new User
            {
                UserId = 1,
                Email = "owner@test.com",
                Fullname = "Station Owner",
                PhoneNumber = "0123456789"
            };

            // Create charging station
            var station = new ChargingStation
            {
                StationId = 1,
                OwnerId = 1,
                StationLocationId = 1,
                StationName = "Test Station",
                Status = "Active",
                MaxConsumPower = 100,
                CreateAt = DateTime.Now
            };

            // Create charging point
            var chargingPoint = new ChargingPoint
            {
                ChargingPointId = 1,
                StationId = 1,
                ChargingPointName = "CP001",
                Description = "Test Charging Point",
                Status = "Available",
                MaxPower = 50,
                CreateAt = DateTime.Now
            };

            // Create car model
            var carModel = new CarModel
            {
                CarModelId = 1,
                Type = "Sedan",
                Color = "Blue",
                SeatNumber = 5,
                Brand = "Tesla",
                BatteryCapacity = 75,
                MaxChargingPower = 250,
                AverageRange = 400,
                ChargingStandard = "Type 2",
                Img = "car.jpg",
                CreateAt = DateTime.Now
            };

            // Create car
            var car = new Car
            {
                CarId = 1,
                CarModelId = 1,
                CarName = "Test Car",
                LicensePlate = "51A-12345",
                IsDeleted = false,
                CreateAt = DateTime.Now
            };

            // Create user car relationship
            var userCar = new UserCar
            {
                UserId = 1,
                CarId = 1,
                Role = "Owner",
                IsAllowedToCharge = true,
                StartDate = DateTime.Now.AddDays(-30)
            };

            // Create real-time data entries
            var realTimeData1 = new RealTimeDatum
            {
                DataId = 1,
                CarId = 1,
                ChargingpointId = 1,
                BatteryLevel = "50%",
                ChargingPower = "45 kW",
                Temperature = "30°C",
                TimeMoment = DateTime.Now.AddHours(-2),
                BatteryVoltage = "400V",
                ChargingCurrent = "112.5A",
                ChargingTime = "00:30:00",
                EnergyConsumed = "22.5 kWh",
                Cost = "450000",
                Powerpoint = "A1",
                Status = "Charging",
                LicensePlate = "51A-12345"
            };

            var realTimeData2 = new RealTimeDatum
            {
                DataId = 2,
                CarId = 1,
                ChargingpointId = 1,
                BatteryLevel = "75%",
                ChargingPower = "35 kW",
                Temperature = "32°C",
                TimeMoment = DateTime.Now.AddHours(-1),
                BatteryVoltage = "400V",
                ChargingCurrent = "87.5A",
                ChargingTime = "01:00:00",
                EnergyConsumed = "35 kWh",
                Cost = "700000",
                Powerpoint = "A1",
                Status = "Charging",
                LicensePlate = "51A-12345"
            };

            // Create charging sessions
            var chargingSession = new ChargingSession
            {
                SessionId = 1,
                CarId = 1,
                ChargingPointId = 1,
                StartTime = DateTime.Now.AddDays(-1),
                EndTime = DateTime.Now.AddDays(-1).AddHours(2),
                Cost = 500000,
                Status = "Completed",
                EnergyConsumed = 25
            };

            // Add entities to context
            await _context.StationLocations.AddAsync(stationLocation);
            await _context.Users.AddAsync(owner);
            await _context.ChargingStations.AddAsync(station);
            await _context.ChargingPoints.AddAsync(chargingPoint);
            await _context.CarModels.AddAsync(carModel);
            await _context.Cars.AddAsync(car);
            await _context.UserCars.AddAsync(userCar);
            await _context.RealTimeData.AddRangeAsync(realTimeData1, realTimeData2);
            await _context.ChargingSessions.AddAsync(chargingSession);
            await _context.SaveChangesAsync();
        }

        [Test]
        public void GetCarByOwner_ValidUserId_ReturnsCarList()
        {
            // Act
            var result = _carService.GetCarByOwner(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].CarId, Is.EqualTo(1));
            Assert.That(result[0].CarName, Is.EqualTo("Test Car"));
            Assert.That(result[0].LicensePlate, Is.EqualTo("51A-12345"));
            Assert.That(result[0].Brand, Is.EqualTo("Tesla"));
        }

        [Test]
        public void GetCarByOwner_InvalidUserId_ReturnsEmptyList()
        {
            // Act
            var result = _carService.GetCarByOwner(999);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetCarDetailById_ValidCarId_ReturnsCarDetail()
        {
            // Act
            var result = _carService.GetCarDetailById(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.CarId, Is.EqualTo(1));
                Assert.That(result.CarName, Is.EqualTo("Test Car"));
                Assert.That(result.LicensePlate, Is.EqualTo("51A-12345"));
                Assert.That(result.CarModelId, Is.EqualTo(1));
                Assert.That(result.Brand, Is.EqualTo("Tesla"));
                Assert.That(result.Type, Is.EqualTo("Sedan"));
                Assert.That(result.Color, Is.EqualTo("Blue"));
            });
        }

        [Test]
        public void GetCarDetailById_InvalidCarId_ReturnsNull()
        {
            // Act
            var result = _carService.GetCarDetailById(999);

            // Assert
            Assert.That(result, Is.Null);
        }


        [Test]
        public void GetChargingStatusById_ValidCarId_ReturnsChargingStatus()
        {
            // Act
            var result = _carService.GetChargingStatusById(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.CarId, Is.EqualTo(1));
                Assert.That(result.ChargingPointId, Is.EqualTo(1));
                Assert.That(result.StationId, Is.EqualTo(1));
                Assert.That(result.StationName, Is.EqualTo("Test Station"));
                Assert.That(result.BatteryLevel, Is.EqualTo("75%"));
                Assert.That(result.Status, Is.EqualTo("Charging"));
            });
        }

        [Test]
        public void GetChargingStatusById_InvalidCarId_ReturnsNull()
        {
            // Act
            var result = _carService.GetChargingStatusById(999);

            // Assert
            Assert.That(result, Is.Null);
        }


        [Test]
        public void GetChargingHistory_ValidCarId_ReturnsHistoryList()
        {
            // Act
            var result = _carService.GetChargingHistory(1, null, null, null);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].SessionId, Is.EqualTo(1));
            Assert.That(result[0].CarId, Is.EqualTo(1));
            Assert.That(result[0].Status, Is.EqualTo("Completed"));
        }

        [Test]
        public void GetChargingHistory_InvalidCarId_ReturnsEmptyList()
        {
            // Act
            var result = _carService.GetChargingHistory(999, null, null, null);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetChargingHistory_WithDateRange_ReturnsFilteredResults()
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(-2);
            var endDate = DateTime.Now;

            // Act
            var result = _carService.GetChargingHistory(1, startDate, endDate, null);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }


        [Test]
        public void GetCarModels_NoSearch_ReturnsAllModels()
        {
            // Act
            var result = _carService.GetCarModels(null);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].CarModelId, Is.EqualTo(1));
            Assert.That(result[0].Brand, Is.EqualTo("Tesla"));
        }

        [Test]
        public void GetCarModels_WithSearch_ReturnsFilteredModels()
        {
            // Act
            var result = _carService.GetCarModels("tesla");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Brand, Is.EqualTo("Tesla"));
        }


        [Test]
        public void AddCar_ValidData_AddsNewCar()
        {
            // Arrange
            int initialCarCount = _context.Cars.Count();

            // Act
            _carService.addCar(1, 1, "51B-123.45", "New Test Car");

            // Assert
            var cars = _context.Cars.ToList();
            Assert.That(cars.Count, Is.EqualTo(initialCarCount + 1));

            var newCar = cars.FirstOrDefault(c => c.LicensePlate == "51B-123.45");
            Assert.That(newCar, Is.Not.Null);
            Assert.That(newCar!.CarName, Is.EqualTo("New Test Car"));
        }

        [Test]
        public void AddCar_InvalidLicensePlate_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _carService.addCar(1, 1, "Invalid-Plate", "Test Car"));

            Assert.That(ex.Message, Is.EqualTo("Invalid license plate format"));
        }

        

        [Test]
        public void EditCar_ValidData_UpdatesCarProperties()
        {
            // Act
            _carService.editCar(1, 1, "51C-789.45", "Updated Car Name");
            var car = _context.Cars.Find(1);

            // Assert
            Assert.That(car!.LicensePlate, Is.EqualTo("51C-789.45"));
            Assert.That(car.CarName, Is.EqualTo("Updated Car Name"));
        }

        [Test]
        public void EditCar_InvalidLicensePlate_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _carService.editCar(1, 1, "Invalid-Format", "Updated Car"));

            Assert.That(ex.Message, Is.EqualTo("Invalid license plate format"));
        }

        [Test]
        public async Task SendRentRequestForRent_ValidRequest_AddsRequest()
        {
            // Arrange
            int initialUserCarCount = _context.UserCars.Count();
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(7);

            // Act
            await _carService.SendRentRequestForRent(2, 1, startDate, endDate);

            // Assert
            Assert.That(_context.UserCars.Count(), Is.EqualTo(initialUserCarCount + 1));

            var request = _context.UserCars.FirstOrDefault(uc =>
                uc.UserId == 2 && uc.CarId == 1 && uc.Role == "Renter");

            Assert.That(request, Is.Not.Null);
            Assert.That(request!.StartDate, Is.EqualTo(startDate));
            Assert.That(request.EndDate, Is.EqualTo(endDate));
            Assert.That(request.IsAllowedToCharge, Is.False);
        }

        [Test]
        public void SendRentRequestForRent_EndDateBeforeStartDate_ThrowsArgumentException()
        {
            // Arrange
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(-1); // End date before start date

            // Act & Assert
            var ex = Assert.Throws<AggregateException>(() =>
                _carService.SendRentRequestForRent(2, 1, startDate, endDate).Wait());

            Assert.That(ex.Message, Does.Contain("Start date must be before end date"));
        }

        [Test]
        public async Task GetRentRequest_WithActiveRental_ReturnsRentRequests()
        {
            // Arrange
            var renter = new User
            {
                UserId = 2,
                Email = "renter@test.com",
                Fullname = "Test Renter",
                PhoneNumber = "0987654321"
            };
            await _context.Users.AddAsync(renter);

            var rentalRequest = new UserCar
            {
                UserId = 2,
                CarId = 1,
                Role = "Renter",
                IsAllowedToCharge = true,
                StartDate = DateTime.Now.AddDays(-3),
                EndDate = DateTime.Now.AddDays(3) // Current rental period
            };
            await _context.UserCars.AddAsync(rentalRequest);
            await _context.SaveChangesAsync();

            // Act
            var result = await _carService.GetRentRequest(2);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].DriverId, Is.EqualTo(2));
                Assert.That(result[0].OwnerId, Is.EqualTo(1));
                Assert.That(result[0].CarId, Is.EqualTo(1));
                Assert.That(result[0].LicensePlate, Is.EqualTo("51A-12345"));
                Assert.That(result[0].Brand, Is.EqualTo("Tesla"));
            });
        }

        [Test]
        public async Task ConfirmRentalAsync_ValidRental_ReturnsTrue()
        {
            // Arrange
            var renter = new User { UserId = 3, Email = "test@example.com", PhoneNumber="0326362145" };
            await _context.Users.AddAsync(renter);

            var rental = new UserCar
            {
                UserId = 3,
                CarId = 1,
                Role = "Renter",
                IsAllowedToCharge = false, // Initially not allowed
                StartDate = DateTime.Now
            };
            await _context.UserCars.AddAsync(rental);
            await _context.SaveChangesAsync();

            // Act
            var result = await _carService.ConfirmRentalAsync(3, 1, "Renter");
            var updatedRental = await _context.UserCars.FindAsync(3, 1);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(updatedRental!.IsAllowedToCharge, Is.True);
        }

        [Test]
        public async Task ConfirmRentalAsync_InvalidRental_ReturnsFalse()
        {
            // Act
            var result = await _carService.ConfirmRentalAsync(999, 1, "Renter");

            // Assert
            Assert.That(result, Is.False);
        }

    }
}
