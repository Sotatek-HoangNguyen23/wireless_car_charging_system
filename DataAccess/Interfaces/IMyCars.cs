using DataAccess.DTOs;
using DataAccess.DTOs.UserDTO;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IMyCars
    {
        List<MyCarsDTO> getCarByOwner(int userId);

        CarDetailDTO getCarDetailById(int carId);

        ChargingStatusDTO GetChargingStatusById(int carId);


        List<ChargingHistoryDTO> GetChargingHistory(int carId, DateTime? start, DateTime? end, int? chargingStationId);

        bool deleteCar(int carId);

        List<CarModel> getCarModels(string? search);

        bool checkDuplicateLicensePlate(string licensePlate) ;

        void addCar(int carModel, int userId, string licensePlate, string carName);

        void editCar(int carModel, int carId, string licensePlate, string carName);

        Task SendRentRequestForRent(UserCar userCar);

        Task<List<RentConfirmDto>> GetRentRequest(int driverId);

        Task<UserCar?> GetUserCarAsync(int userId, int carId, string role);
        Task<bool> UpdateIsAllowedToChargeAsync(int userId, int carId, string role);

        PagedResult<CarDetailDTO> GetAllCars(string? search, string? type, string? brand, bool? status, int page, int pageSize);

        List<string?> GetAllBrands();
        List<string?> GetAllTypes();
    }
}
