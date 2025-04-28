using DataAccess.DTOs;
using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using DataAccess.Repositories;
using System.Text.RegularExpressions;

namespace API.Services
{
    public class CarService
    {
        private readonly IMyCars _myCars;

        public CarService(IMyCars myCar)
        {
            _myCars = myCar;
        }

        public List<MyCarsDTO> GetCarByOwner( int userId)
        {
            return _myCars.getCarByOwner(userId);
        }

        public CarDetailDTO GetCarDetailById(int carId)
        {
            return _myCars.getCarDetailById(carId);
        }

        public ChargingStatusDTO GetChargingStatusById(int carId) {
            return _myCars.GetChargingStatusById(carId);
        }

        public List<ChargingHistoryDTO> GetChargingHistory(int carId, DateTime? start, DateTime? end, int? chargingStationId)
        {
            return _myCars.GetChargingHistory(carId, start, end, chargingStationId);
        }

        public void deleteCar(int carId)
        {
             _myCars.deleteCar(carId);
        }

        public List<CarModel> GetCarModels(string? search) { 
           return _myCars.getCarModels(search);
        }

        public void addCar(int carModel, int userId, string licensePlate, string carName)
        {
            if (!IsValidVietnameseLicensePlate(licensePlate))
            {
                throw new ArgumentException("Invalid license plate format");
            }

            if (_myCars.checkDuplicateLicensePlate(licensePlate))
            {
                throw new ArgumentException("License Plate already exists");
            }

            try
            {
                _myCars.addCar(carModel, userId, licensePlate, carName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
        }

        public void editCar(int carModel, int carId, string licensePlate, string carName) {
            _myCars.editCar(carModel,carId,licensePlate,carName);
        }

        private bool IsValidVietnameseLicensePlate(string licensePlate)
        {
            string pattern = @"^\d{2}[A-Z]{1,2}-\d{3}\.\d{2}$";
            return Regex.IsMatch(licensePlate, pattern);
        }

        public async Task SendRentRequestForRent(int userId, int carId, DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
            {
                throw new ArgumentException("Start date must be before end date.");
            }
            var userCar = new UserCar
            {
                UserId = userId,
                CarId = carId,
                Role = "Renter",
                IsAllowedToCharge = false,
                StartDate = startDate,
                EndDate = endDate
            };

            await _myCars.SendRentRequestForRent(userCar);
        }

        public async Task<List<RentConfirmDto>> GetRentRequest(int driverId)
        {
            return await _myCars.GetRentRequest(driverId);
        }

        public async Task<bool> ConfirmRentalAsync(int userId, int carId, string role)
        {
            return await _myCars.UpdateIsAllowedToChargeAsync(userId, carId, role);
        }

        public PagedResult<CarDetailDTO> GetAllCars(string? search, string? type, string? brand, bool? status, int page, int pageSize)
        {
            return _myCars.GetAllCars(search, type, brand, status, page, pageSize);
        }

        public CarFilterOptionsDto GetFilterOptions()
        {
            return new CarFilterOptionsDto
            {
                Brands = _myCars.GetAllBrands(),
                Types = _myCars.GetAllTypes()
            };
        }
    }
}
