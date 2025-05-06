using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.CarRepo;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TestProject.MyCarTest
{
    [TestFixture]
    public class MyCarRepoTest
    {
        private WccsContext _context;
        private MyCarsRepo _repository;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            

            _context = new WccsContext(options);
            _repository = new MyCarsRepo(_context);

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
        public void GetChargingStatusById_ValidCarId_ReturnsLatestChargingStatus()
        {
            // Act
            var result = _repository.GetChargingStatusById(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.CarId, Is.EqualTo(1));
                Assert.That(result.ChargingPointId, Is.EqualTo(1));
                Assert.That(result.StationId, Is.EqualTo(1));
                Assert.That(result.StationName, Is.EqualTo("Test Station"));
                Assert.That(result.Address, Is.EqualTo("123 Test Street"));
                Assert.That(result.BatteryLevel, Is.EqualTo("75%")); 
                Assert.That(result.Status, Is.EqualTo("Charging"));
            });
        }

        [Test]
        public void GetChargingStatusById_InvalidCarId_ReturnsNull()
        {
            // Act
            var result = _repository.GetChargingStatusById(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetChargingStatusById_MultipleRecords_ReturnsLatestTimeRecord()
        {
            // Arrange
            var newerData = new RealTimeDatum
            {
                DataId = 3,
                CarId = 1,
                ChargingpointId = 1,
                BatteryLevel = "90%",
                ChargingPower = "20 kW",
                Temperature = "29°C",
                TimeMoment = DateTime.Now, // Newest record
                BatteryVoltage = "398V",
                ChargingCurrent = "50.3A",
                ChargingTime = "01:30:00",
                EnergyConsumed = "45 kWh",
                Cost = "900000",
                Powerpoint = "A1",
                Status = "Charging",
                LicensePlate = "51A-12345"
            };

            await _context.RealTimeData.AddAsync(newerData);
            await _context.SaveChangesAsync();

            // Act
            var result = _repository.GetChargingStatusById(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.BatteryLevel, Is.EqualTo("90%"));
                Assert.That(result.Status, Is.EqualTo("Charging"));
            });
        }

        // Tests for GetChargingHistory
        [Test]
        public void GetChargingHistory_ValidCarId_ReturnsChargingHistory()
        {
            // Act
            var result = _repository.GetChargingHistory(1, null, null, null);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SessionId, Is.EqualTo(1));
            Assert.That(result[0].CarId, Is.EqualTo(1));
            Assert.That(result[0].Status, Is.EqualTo("Completed"));
        }

        [Test]
        public void GetChargingHistory_InvalidCarId_ReturnsEmptyList()
        {
            // Act
            var result = _repository.GetChargingHistory(999, null, null, null);

            // Assert
            Assert.That(result, Is.Empty);
        }
        [Test]
        public async Task GetChargingHistory_WithDateFilter_ReturnsFilteredResults()
        {
            // Arrange
            var newSession = new ChargingSession
            {
                SessionId = 2,
                CarId = 1,
                ChargingPointId = 1,
                StartTime = DateTime.Now.AddDays(-10),
                EndTime = DateTime.Now.AddDays(-10).AddHours(1),
                Cost = 300000,
                Status = "Completed",
                EnergyConsumed = 15
            };
            await _context.ChargingSessions.AddAsync(newSession);
            await _context.SaveChangesAsync();

            // Act - Filter for only recent sessions
            var result = _repository.GetChargingHistory(
                1,
                DateTime.Now.AddDays(-5), // Start date
                null,
                null
            );

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SessionId, Is.EqualTo(1));
        }

        [Test]
        public async Task GetChargingHistory_WithStationFilter_ReturnsFilteredResults()
        {
            // Arrange
            var newStation = new ChargingStation
            {
                StationId = 2,
                OwnerId = 1,
                StationLocationId = 1,
                StationName = "Second Station",
                Status = "Active"
            };
            await _context.ChargingStations.AddAsync(newStation);

            var newPoint = new ChargingPoint
            {
                ChargingPointId = 2,
                StationId = 2,
                ChargingPointName = "CP002",
                Status = "Available"
            };
            await _context.ChargingPoints.AddAsync(newPoint);

            var newSession = new ChargingSession
            {
                SessionId = 2,
                CarId = 1,
                ChargingPointId = 2, // Different charging point at different station
                StartTime = DateTime.Now.AddDays(-2),
                EndTime = DateTime.Now.AddDays(-2).AddHours(1),
                Cost = 350000,
                Status = "Completed",
                EnergyConsumed = 17.5
            };
            await _context.ChargingSessions.AddAsync(newSession);
            await _context.SaveChangesAsync();

            // Act - Filter by station ID
            var result = _repository.GetChargingHistory(
                1,
                null,
                null,
                2 // Station ID
            );

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SessionId, Is.EqualTo(2));
            Assert.That(result[0].StationId, Is.EqualTo(2));
        }

        // Tests for deleteCar
        [Test]
        public void DeleteCar_ValidCarId_ReturnsTrueAndSetsIsDeleted()
        {
            // Act
            var result = _repository.deleteCar(1);
            var car = _context.Cars.Find(1);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(car!.IsDeleted, Is.True);
        }

        [Test]
        public void DeleteCar_InvalidCarId_ReturnsFalse()
        {
            // Act
            var result = _repository.deleteCar(999);

            // Assert
            Assert.That(result, Is.False);
        }

        // Tests for getCarModels
        [Test]
        public void GetCarModels_NoSearch_ReturnsAllModels()
        {
            // Act
            var result = _repository.getCarModels(null);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].CarModelId, Is.EqualTo(1));
        }

        [Test]
        public void GetCarModels_WithSearch_ReturnsFilteredModels()
        {
            // Arrange
            _context.CarModels.Add(new CarModel
            {
                CarModelId = 2,
                Type = "SUV",
                Color = "Red",
                Brand = "BMW"
            });
            _context.SaveChanges();

            // Act - Search by type
            var result1 = _repository.getCarModels("sedan");
            // Act - Search by brand
            var result2 = _repository.getCarModels("BMW");

            // Assert
            Assert.That(result1, Has.Count.EqualTo(1));
            Assert.That(result1[0].Type, Is.EqualTo("Sedan"));

            Assert.That(result2, Has.Count.EqualTo(1));
            Assert.That(result2[0].Brand, Is.EqualTo("BMW"));
        }

        [Test]
        public void GetCarModels_CaseInsensitiveSearch_FindsMatches()
        {
            // Act
            var result = _repository.getCarModels("TESLA");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Brand, Is.EqualTo("Tesla"));
        }

        // Tests for checkDuplicateLicensePlate
        [Test]
        public void CheckDuplicateLicensePlate_ExistingPlate_ReturnsTrue()
        {
            // Act
            var result = _repository.checkDuplicateLicensePlate("51A-12345");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CheckDuplicateLicensePlate_NonExistingPlate_ReturnsFalse()
        {
            // Act
            var result = _repository.checkDuplicateLicensePlate("51A-99999");

            // Assert
            Assert.That(result, Is.False);
        }

        // Tests for addCar
        //[Test]
        //public void AddCar_ValidData_AddsCarAndUserCarRelationship()
        //{
        //    // Arrange
        //    int initialCarCount = _context.Cars.Count();
        //    int initialUserCarCount = _context.UserCars.Count();

        //    // Act
        //    _repository.addCar(1, 1, "51B-54321", "New Test Car");

        //    // Assert
        //    Assert.That(_context.Cars.Count(), Is.EqualTo(initialCarCount + 1));
        //    Assert.That(_context.UserCars.Count(), Is.EqualTo(initialUserCarCount + 1));

        //    var newCar = _context.Cars.FirstOrDefault(c => c.LicensePlate == "51B-54321");
        //    Assert.That(newCar, Is.Not.Null);
        //    Assert.That(newCar!.CarName, Is.EqualTo("New Test Car"));

        //    var userCar = _context.UserCars.FirstOrDefault(uc => uc.CarId == newCar.CarId);
        //    Assert.That(userCar, Is.Not.Null);
        //    Assert.That(userCar!.UserId, Is.EqualTo(1));
        //    Assert.That(userCar.Role, Is.EqualTo("Owner"));
        //    Assert.That(userCar.IsAllowedToCharge, Is.True);
        //}

        //[Test]
        //public void AddCar_InvalidCarModelId_ThrowsException()
        //{
        //    // Act & Assert
        //    var ex = Assert.Throws<Exception>(() =>
        //        _repository.addCar(999, 1, "51B-12345", "Invalid Model Car"));

        //    Assert.That(ex.Message, Does.Contain("Error add car"));
        //}

        // Tests for editCar
        [Test]
        public void EditCar_ValidData_UpdatesCarProperties()
        {
            // Act
            _repository.editCar(1, 1, "51C-98765", "Updated Car Name");
            var car = _context.Cars.Find(1);

            // Assert
            Assert.That(car!.LicensePlate, Is.EqualTo("51C-98765"));
            Assert.That(car.CarName, Is.EqualTo("Updated Car Name"));
        }

        [Test]
        public void EditCar_InvalidCarId_DoesNothing()
        {
            // Arrange
            var initialState = _context.Cars.Find(1);
            var originalLicensePlate = initialState!.LicensePlate;
            var originalName = initialState.CarName;

            // Act - Try to edit non-existent car
            _repository.editCar(1, 999, "51D-11111", "Should Not Update");
            var car = _context.Cars.Find(1);

            // Assert - Original car should remain unchanged
            Assert.That(car!.LicensePlate, Is.EqualTo(originalLicensePlate));
            Assert.That(car.CarName, Is.EqualTo(originalName));
        }

        // Tests for SendRentRequestForRent
        [Test]
        public async Task SendRentRequestForRent_ValidRequest_AddsToDatabase()
        {
            // Arrange
            var rentRequest = new UserCar
            {
                UserId = 2,
                CarId = 1,
                Role = "Renter",
                IsAllowedToCharge = false,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7)
            };
            await _context.Users.AddAsync(new User { UserId = 2, Email = "renter@test.com", PhoneNumber = "0326548569" });
            await _context.SaveChangesAsync();

            int initialCount = _context.UserCars.Count();

            // Act
            await _repository.SendRentRequestForRent(rentRequest);

            // Assert
            Assert.That(_context.UserCars.Count(), Is.EqualTo(initialCount + 1));
            var savedRequest = await _context.UserCars.FindAsync(2, 1, "Renter");
            Assert.That(savedRequest, Is.Not.Null);
        }

        // Tests for UpdateIsAllowedToChargeAsync
        [Test]
        public async Task UpdateIsAllowedToChargeAsync_ExistingRelationship_ReturnsTrueAndUpdates()
        {
            // Arrange
            var userCar = await _context.UserCars.FindAsync(1, 1);
            userCar!.IsAllowedToCharge = false;
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.UpdateIsAllowedToChargeAsync(1, 1, "Owner");
            var updated = await _context.UserCars.FindAsync(1, 1);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(updated!.IsAllowedToCharge, Is.True);
        }

        [Test]
        public async Task UpdateIsAllowedToChargeAsync_NonExistingRelationship_ReturnsFalse()
        {
            // Act
            var result = await _repository.UpdateIsAllowedToChargeAsync(999, 1, "Owner");

            // Assert
            Assert.That(result, Is.False);
        }

        //getrentrequest
        [Test]
        public async Task GetRentRequest_ReturnsCorrectRentRequests()
        {
            // Arrange
            var secondUser = new User
            {
                UserId = 2,
                Email = "renter@test.com",
                Fullname = "Test Renter",
                PhoneNumber = "0987654321"
            };
            await _context.Users.AddAsync(secondUser);

            var userCarRentRequest = new UserCar
            {
                UserId = 2, // Renter
                CarId = 1,
                Role = "Renter",
                IsAllowedToCharge = true,
                StartDate = DateTime.Now.AddDays(-3),
                EndDate = DateTime.Now.AddDays(3) // Current rent period
            };
            await _context.UserCars.AddAsync(userCarRentRequest);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRentRequest(2);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(result[0].DriverId, Is.EqualTo(2));
                Assert.That(result[0].OwnerId, Is.EqualTo(1));
                Assert.That(result[0].CarId, Is.EqualTo(1));
                Assert.That(result[0].LicensePlate, Is.EqualTo("51A-12345"));
                Assert.That(result[0].IsAllowedToCharge, Is.True);
                Assert.That(result[0].Type, Is.EqualTo("Sedan"));
                Assert.That(result[0].Brand, Is.EqualTo("Tesla"));
            });
        }

        [Test]
        public async Task GetRentRequest_WithPastRental_ReturnsEmptyList()
        {
            // Arrange
            var renter = new User
            {
                UserId = 3,
                Email = "past.renter@test.com",
                Fullname = "Past Renter",
                PhoneNumber = "0555555555"
            };
            await _context.Users.AddAsync(renter);

            var pastRental = new UserCar
            {
                UserId = 3,
                CarId = 1,
                Role = "Renter",
                IsAllowedToCharge = true,
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(-5) // Past rental period
            };
            await _context.UserCars.AddAsync(pastRental);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRentRequest(3);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetRentRequest_WithFutureRental_ReturnsEmptyList()
        {
            // Arrange
            var renter = new User
            {
                UserId = 4,
                Email = "future.renter@test.com",
                Fullname = "Future Renter",
                PhoneNumber = "0666666666"
            };
            await _context.Users.AddAsync(renter);

            var futureRental = new UserCar
            {
                UserId = 4,
                CarId = 1,
                Role = "Renter",
                IsAllowedToCharge = true,
                StartDate = DateTime.Now.AddDays(5),
                EndDate = DateTime.Now.AddDays(10) // Future rental period
            };
            await _context.UserCars.AddAsync(futureRental);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRentRequest(4);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task IsCarBeingRentedAsync_CurrentlyRented_ReturnsTrue()
        {
            // Arrange
            var renter = new User
            {
                UserId = 6,
                Email = "active.renter@test.com",
                Fullname = "Active Renter",
                PhoneNumber = "0888888888"
            };
            await _context.Users.AddAsync(renter);

            var activeRental = new UserCar
            {
                UserId = 6,
                CarId = 1,
                Role = "Renter",
                IsAllowedToCharge = true,
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(3)
            };
            await _context.UserCars.AddAsync(activeRental);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsCarBeingRentedAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsCarBeingRentedAsync_NotRented_ReturnsFalse()
        {
            // Arrange - We already have car with ID 1, but no active rentals for car ID 2
            var car2 = new Car
            {
                CarId = 3,
                CarModelId = 1,
                CarName = "Not Rented Car",
                LicensePlate = "51C-99999",
                IsDeleted = false
            };
            await _context.Cars.AddAsync(car2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsCarBeingRentedAsync(3);

            // Assert
            Assert.That(result, Is.False);
        }

    }
}
