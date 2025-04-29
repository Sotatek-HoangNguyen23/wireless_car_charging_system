using DataAccess.DTOs.CarDTO;
using DataAccess.DTOs.ChargingStation;
using DataAccess.DTOs;
using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using DataAccess.Repositories;
using System.Text.RegularExpressions;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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

        public List<ChargingHistoryDTO> GetChargingHistory(int carId, DateTime? start, DateTime? end, int? chargingStationId, int page = 1, int pageSize = 10)
        {
            return _myCars.GetChargingHistory(carId, start, end, chargingStationId, page, pageSize);
        }


        //public ChargingCarStatsDto GetCarStats(int carId, int? year, int? month)
        //{
        //    var sessions = _myCars.GetChargingHistoryByCarId(carId);

        //    if (year.HasValue)
        //        sessions = sessions.Where(s => s.StartTime.Value.Year == year.Value).ToList();

        //    if (month.HasValue)
        //        sessions = sessions.Where(s => s.StartTime.Value.Month == month.Value).ToList();

        //    double totalEnergy = sessions.Sum(s => s.EnergyConsumed) ?? 0;
        //    double totalRevenue = sessions.Sum(s => s.Cost) ?? 0;
        //    int totalSessions = sessions.Count;
        //    double avgTime = sessions.Average(s => (s.EndTime - s.StartTime)?.TotalMinutes) ?? 0;

        //    var chartData = month.HasValue
        //        ? sessions.GroupBy(s => s.StartTime.Value.Day)
        //                  .Select(g => new ChartDataDto
        //                  {
        //                      Label = $"Ngày {g.Key}",
        //                      Revenue = Math.Round(g.Sum(s => s.Cost) ?? 0, 2),
        //                      SessionCount = g.Count()
        //                  }).ToList()
        //        : sessions.GroupBy(s => s.StartTime.Value.Month)
        //                  .Select(g => new ChartDataDto
        //                  {
        //                      Label = $"Tháng {g.Key}",
        //                      Revenue = Math.Round(g.Sum(s => s.Cost) ?? 0, 2),
        //                      SessionCount = g.Count()
        //                  }).ToList();

        //    return new ChargingCarStatsDto
        //    {
        //        TotalEnergyConsumed = Math.Round(totalEnergy, 2),
        //        TotalRevenue = Math.Round(totalRevenue, 2),
        //        TotalChargingSessions = totalSessions,
        //        AverageChargingTime = Math.Round(avgTime, 2),
        //        ChartData = chartData
        //    };
        //}

        public List<CarMonthlyStatDTO> GetCarStats(int carId, int? year)
        {
            return _myCars.GetCarStats(carId, year ?? DateTime.Now.Year);
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
            if (!IsValidVietnameseLicensePlate(licensePlate))
            {
                throw new ArgumentException("Invalid license plate format");
            }

            if (_myCars.CheckDuplicateLicensePlateForEdit(carId,licensePlate))
            {
                throw new ArgumentException("License Plate already exists");
            }

            try
            {
                _myCars.editCar(carModel, carId, licensePlate, carName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
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

        //public async Task<ChargingSession> AddChargingSession(int carId, int pointId, string timeMoment, string chargingTime,string energy,string cost)
        //{
        //    // Parse thời gian kết thúc (end time)
        //    if (!DateTime.TryParse(timeMoment, out DateTime endTime))
        //        throw new ArgumentException("Invalid timeMoment format");

        //    // Parse thời gian sạc (charging time) thành TimeSpan
        //    if (!TryParseChargingTime(chargingTime, out TimeSpan duration))
        //        throw new ArgumentException("Invalid chargingTime format");

        //    DateTime startTime = endTime - duration;

        //    // Parse energy & cost
        //    double.TryParse(energy, out double energyConsumed);
        //    double.TryParse(cost, out double costValue);

        //    int userId = (int)await _myCars.GetCurrentDriverByCarId(carId);

        //    var session = new ChargingSession
        //    {
        //        CarId = carId,
        //        ChargingPointId = pointId,
        //        UserId = userId,
        //        StartTime = startTime,
        //        EndTime = endTime,
        //        EnergyConsumed = energyConsumed,
        //        Cost = costValue,
        //        Status = "Completed"
        //    };

        //    return await _myCars.AddChargingSession(session);
        //}

        //private bool TryParseChargingTime(string? timeStr, out TimeSpan timeSpan)
        //{
        //    timeSpan = TimeSpan.Zero;
        //    if (string.IsNullOrEmpty(timeStr)) return false;

        //    var match = Regex.Match(timeStr, @"(?:(\d+)h)?\s*(?:(\d+)m)?\s*(?:(\d+)s)?");
        //    if (!match.Success) return false;

        //    int h = int.TryParse(match.Groups[1].Value, out var hour) ? hour : 0;
        //    int m = int.TryParse(match.Groups[2].Value, out var min) ? min : 0;
        //    int s = int.TryParse(match.Groups[3].Value, out var sec) ? sec : 0;

        //    timeSpan = new TimeSpan(h, m, s);
        //    return true;
        //}

        public Task<bool> IsCarBeingRentedAsync(int carId)
        {
            return _myCars.IsCarBeingRentedAsync(carId);
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
