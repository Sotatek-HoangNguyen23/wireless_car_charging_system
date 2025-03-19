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

namespace TestProject.MyCarTest
{
    public class MyCarRepoTest
    {
        private IMyCars _repository;
        private WccsContext context = new();

        [SetUp]
        public void Setup()
        {
            _repository = new MyCarsRepo();
        }
//get car by Owner
        [Test]
        public void GetAlCar_ShouldReturnCar_WhenOwnerExsist()
        {
            var result = _repository.getCarByOwner(2);

            Assert.That(result.Count, Is.EqualTo(3));

        }

        [Test]
        public void GetCarByOwner_ShouldReturnNull_WhenOwnerNotExsist()
        {
            var cars = _repository.getCarByOwner(0);
            Assert.That(cars, Is.Empty);

        }
        // get car by id
        [Test]
        public void GetCarById_ShouldReturnCorrectCar()
        {
            var car = _repository.getCarDetailById(1);
            Assert.That(car.CarId, Is.EqualTo(1));

        }

        [Test]
        public void GetCarById_ShouldReturnNull_CarNotExsit()
        {
            var car = _repository.getCarDetailById(0);
            Assert.That(car, Is.Null);
        }

        // get charging status by car 
        [Test]
        public void GetChargingStatusById_CarIsCharging()
        {
            
            var result = _repository.GetChargingStatusById(1);
            Assert.That(result, Is.Not.Null);
            
        }

        [Test]
        public void GetChargingStatusById_CarIsNotCharging()
        {

            var result = _repository.GetChargingStatusById(2);
            Assert.That(result, Is.Null);

        }

        //get charging history
        [Test]
        public void GetChargingHistory_ByCarId_WhenCarNotExsist()
        {
            var result = _repository.GetChargingHistory(0,null,null,null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetHistoryCharging_WhenChargingStationNotExist()
        {
            var result = _repository.GetChargingHistory(1, null, null, 0);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetHistoryCharging_WhenTimeNotValid()
        {
            var result = _repository.GetChargingHistory(1, DateTime.Now.AddYears(1), null, null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetHistoryCharging_WhenHistoryExist()
        {
            var result = _repository.GetChargingHistory(1,new DateTime(2025, 2, 20),new DateTime(2025, 2, 25), null);

            Assert.That(result.Count, Is.EqualTo(3));

        }

        //delete car
        [Test]
        public void DeleteCar_WhenCarIdNotExist()
        {
            var result = _repository.deleteCar(999);
            Assert.That(result, Is.False);
        }

        public void DeleteCar_WhenCarIdExist()
        {
            var result = _repository.deleteCar(5);
            Assert.That(result, Is.True);
        }

        // get car model
        [Test]
        public void GetAllCarModel_ByName_Success()
        {
            var result = _repository.getCarModels("Vin");
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetAllCarModel_ByColor_Success()
        {
            var result = _repository.getCarModels("Red");
            Assert.That(result.Count, Is.EqualTo(1));
        }


        [Test]
        public void GetAllCarModel_ByBrand_Success()
        {
            var result = _repository.getCarModels("VinFast");
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetAllCarModel_ByColor_NotExist()
        {
            var result = _repository.getCarModels("Pink");
            Assert.That(result.Count, Is.EqualTo(0));
        }

        //check duplicate license plate
        [Test]
        public void CheckDuplicateLicensePlate_WhenDuplicate()
        {
            var result = _repository.checkDuplicateLicensePlate("51B-67890");
            Assert.That(result, Is.True);
        }

        public void CheckDuplicateLicensePlate_WhenNotDuplicate()
        {
            var result = _repository.checkDuplicateLicensePlate("51B-67899");
            Assert.That(result, Is.False);
        }

        //add car
        //[Test]
        //public void AddCar_Success()
        //{

        //    int carModel = 2;
        //    int userId = 2;
        //    string licensePlate = "51B-67890";
        //    string carName = "Duplicate Car";

        //    var ex = Assert.Throws<Exception>(() => _repository.addCar(carModel, userId, licensePlate, "Another Car"));
        //    Assert.That(ex.Message, Does.Contain("Error add car"));


        //}

        

    }
}
