using DataAccess.DTOs;
using DataAccess.Models;
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


        List<ChargingHistoryDTO> GetChargingHistory(int carId, DateTime? start, DateTime? end, int? chargingStationId, int page = 1, int pageSize = 10);
        //List<ChargingSession> GetChargingHistoryByCarId(int carId);
        List<CarMonthlyStatDTO> GetCarStats(int carId, int year);
        bool deleteCar(int carId);

        List<CarModel> getCarModels(string? search);

        bool checkDuplicateLicensePlate(string licensePlate) ;

        void addCar(int carModel, int userId, string licensePlate, string carName);

        void editCar(int carModel, int carId, string licensePlate, string carName);

        Task SendRentRequestForRent(UserCar userCar);

        Task<List<RentConfirmDto>> GetRentRequest(int driverId);

        Task<UserCar?> GetUserCarAsync(int userId, int carId, string role);
        Task<bool> UpdateIsAllowedToChargeAsync(int userId, int carId, string role);

        Task<ChargingSession> AddChargingSession(ChargingSession session);

        Task<int?> GetCurrentDriverByCarId( int carId);

        bool CheckDuplicateLicensePlateForEdit(int carId, string newLicensePlate);

        Task<bool> IsCarBeingRentedAsync(int carId);
    }
}
