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


        List<ChargingHistoryDTO> GetChargingHistory(int carId, DateTime? start, DateTime? end, int? chargingStationId);

        bool deleteCar(int carId);

        List<CarModel> getCarModels(string? search);

        bool checkDuplicateLicensePlate(string licensePlate) ;

        void addCar(int carModel, int userId, string licensePlate, string carName);

        void editCar(int carModel, int carId, string licensePlate, string carName);
    }
}
