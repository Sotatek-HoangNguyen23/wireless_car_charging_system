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

namespace TestProject.MyCarTest
{
    [TestFixture]
    public class MyCarRepoTest
    {
        private MyCarsRepo _repository;
        private WccsContext _context;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
         .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
         .Options;

            _context = new WccsContext(options);
            _repository = new MyCarsRepo(_context);

            // Add User
            var user = new User
            {
                UserId = 1,
                Fullname = "Test User",
                Email = "test@example.com",
                PhoneNumber = "0123456789"
            };

            // Add CarModel
            var model = new CarModel
            {
                CarModelId = 1,
                Type = "Model X",
                Brand = "Tesla"
            };

            // Add Car
            var car = new Car
            {
                CarId = 1,
                CarModelId = 1,
                CarName = "My Tesla",
                LicensePlate = "ABC123",
                CarModel = model,
                IsDeleted = false
            };

            // Add UserCar relationship
            var userCar = new UserCar
            {
                UserId = 1,
                CarId = 1,
                Role = "Owner",
                IsAllowedToCharge = true,
                StartDate = DateTime.UtcNow,
                User = user,
                Car = car
            };

            await _context.Users.AddAsync(user);
            await _context.CarModels.AddAsync(model);
            await _context.Cars.AddAsync(car);
            await _context.UserCars.AddAsync(userCar);

            await _context.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
    //---------------------
        [Test]
        public void GetCarByOwner_ExistingUserId_ReturnsCars()
        {
            var result = _repository.getCarByOwner(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].LicensePlate, Is.EqualTo("ABC123"));
        }

        [Test]
        public async Task GetCarByOwner_InvalidUserId_ReturnsEmpty()
        {
            var result =  _repository.getCarByOwner(99);

            Assert.That(result, Is.Empty);
        }
        //---------------------
        [Test]
        public void GetCarDetailById_ValidId_ReturnsDetails()
        {
            var result = _repository.getCarDetailById(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.LicensePlate, Is.EqualTo("ABC123"));
            Assert.That(result.Brand, Is.EqualTo("Tesla"));
        }

        [Test]
        public void GetCarDetailById_InvalidId_ReturnsNull()
        {
            var result = _repository.getCarDetailById(999);

            Assert.That(result, Is.Null);
        }


    }
}
