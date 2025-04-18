using DataAccess.DTOs;
using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.CarRepo
{
    public class MyCarsRepo : IMyCars
    {
        private WccsContext _context;
        public MyCarsRepo()
        {
            _context = new WccsContext();
        }

        public List<MyCarsDTO> getCarByOwner(int userId)
        {
            var cars = (from uc in _context.UserCars
                        join c in _context.Cars on uc.CarId equals c.CarId
                        join cm in _context.CarModels on c.CarModelId equals cm.CarModelId
                        where uc.UserId == userId && uc.Role == "Owner" && c.IsDeleted == false
                        select new MyCarsDTO
                        {
                            CarId = c.CarId,
                            CarName = c.CarName,
                            LicensePlate = c.LicensePlate,
                            IsDeleted = c.IsDeleted,
                            Type = cm.Type,
                            Color = cm.Color,
                            Brand = cm.Brand,
                            Img = cm.Img
                        }).ToList();

            return cars;
        }

        public CarDetailDTO getCarDetailById(int carId)
        {
            var carDetail = (from c in _context.Cars
                             join cm in _context.CarModels on c.CarModelId equals cm.CarModelId
                             where c.CarId == carId && c.IsDeleted == false
                             select new CarDetailDTO
                             {
                                 CarId = c.CarId,
                                 CarName = c.CarName,
                                 LicensePlate = c.LicensePlate,
                                 IsDeleted = c.IsDeleted,
                                 CarCreateAt = c.CreateAt,
                                 CarUpdateAt = c.UpdateAt,
                                 CarModelId = cm.CarModelId,
                                 Type = cm.Type,
                                 Color = cm.Color,
                                 SeatNumber = cm.SeatNumber,
                                 Brand = cm.Brand,
                                 BatteryCapacity = cm.BatteryCapacity,
                                 MaxChargingPower = cm.MaxChargingPower,
                                 AverageRange = cm.AverageRange,
                                 ChargingStandard = cm.ChargingStandard,
                                 Img = cm.Img,
                                 ModelCreateAt = cm.CreateAt,
                                 ModelUpdateAt = cm.UpdateAt
                             }).FirstOrDefault();

            return carDetail;
        }

        public ChargingStatusDTO? GetChargingStatusById(int carId)
        {
            var query = (from cs in _context.ChargingSessions
                         join cp in _context.ChargingPoints on cs.ChargingPointId equals cp.ChargingPointId
                         join s in _context.ChargingStations on cp.StationId equals s.StationId
                         join sl in _context.StationLocations on s.StationLocationId equals sl.StationLocationId
                         join rtd in _context.RealTimeData on cs.CarId equals rtd.CarId
                         where cs.Status == "Charging" && cs.CarId == carId
                         orderby EF.Functions.Collate(rtd.TimeMoment, "SQL_Latin1_General_CP1_CI_AS") descending
                         select new ChargingStatusDTO
                         {
                             SessionId = cs.SessionId,
                             CarId = cs.CarId,
                             ChargingPointId = cs.ChargingPointId,
                             StationId = s.StationId,
                             StationLocationId = s.StationLocationId,
                             StationName = s.StationName,
                             Address = sl.Address,
                             Status = cs.Status,
                             BatteryLevel = rtd.BatteryLevel,
                             ChargingPower = rtd.ChargingPower,
                             Temperature = rtd.Temperature,
                             Cost = rtd.Cost,
                             Current = rtd.ChargingCurrent
                         })
                         .FirstOrDefault();

            return query;
        }


        public List<ChargingHistoryDTO> GetChargingHistory(int carId, DateTime? start, DateTime? end, int? chargingStationId)
        {
            var query = from cs in _context.ChargingSessions
                        join c in _context.Cars on cs.CarId equals c.CarId
                        join cp in _context.ChargingPoints on cs.ChargingPointId equals cp.ChargingPointId
                        join s in _context.ChargingStations on cp.StationId equals s.StationId
                        join sl in _context.StationLocations on s.StationLocationId equals sl.StationLocationId
                        where cs.CarId == carId
                        select new ChargingHistoryDTO
                        {
                            SessionId = cs.SessionId,
                            CarId = cs.CarId,
                            CarName = c.CarName,
                            LicensePlate = c.LicensePlate,
                            ChargingPointId = cs.ChargingPointId,
                            StationName = s.StationName,
                            StationId = cp.StationId,
                            Address = sl.Address,
                            StartTime = cs.StartTime,
                            EndTime = cs.EndTime,
                            Cost = cs.Cost,
                            Status = cs.Status
                        };


            if (start.HasValue && end.HasValue)
            {
                query = query.Where(cs => cs.StartTime >= start.Value && (cs.EndTime <= end.Value ));
            }
            else
            {
                if (start.HasValue && !end.HasValue)
                {
                    query = query.Where(cs => cs.StartTime >= start.Value);
                }

                if (end.HasValue && !start.HasValue)
                {
                    query = query.Where(cs => cs.EndTime <= end.Value );
                }
            }


            if (chargingStationId.HasValue)
            {
                query = query.Where(cs => cs.StationId == chargingStationId.Value);
            }

            return query.OrderByDescending(cs => cs.StartTime).ToList();
        }

        public bool deleteCar(int carId)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);
            if (car != null)
            {
                car.IsDeleted = true;
                _context.SaveChanges();
                return true;
            }
            return false; 
        }

        public List<CarModel> getCarModels(string? search)
        {
            var carModels = _context.CarModels.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                carModels = carModels.Where(c =>
                    (c.Type != null && c.Type.ToLower().Contains(search)) ||
                    (c.Color != null && c.Color.ToLower().Contains(search)) ||
                    (c.Brand != null && c.Brand.ToLower().Contains(search))
                );
            }

            return carModels.ToList();
        }

        public bool checkDuplicateLicensePlate(string licensePlate)
        {
            bool isDuplicate = _context.Cars
                    .Any(c => c.LicensePlate.ToLower() == licensePlate.ToLower() && (c.IsDeleted ?? false) == false);

            return isDuplicate;
            
        }

        public void addCar(int carModel, int userId, string licensePlate, string carName)
        {
            //code here
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var car = new Car
                    {
                        CarModelId = carModel,
                        CarName = carName,
                        LicensePlate = licensePlate,
                        IsDeleted = false,
                        CreateAt = DateTime.Now,
                        UpdateAt = DateTime.Now
                    };

                    _context.Cars.Add(car);
                    _context.SaveChanges();

                    var userCar = new UserCar
                    {
                        UserId = userId,
                        CarId = car.CarId, 
                        Role = "Owner",      
                        IsAllowedToCharge = true,
                        StartDate = DateTime.Now
                    };

                    _context.UserCars.Add(userCar);
                    _context.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Error add car: " + ex.Message);
                }
            }
        }

        public void editCar(int carModel, int carId, string licensePlate, string carName)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);
            if (car != null)
            {
                car.CarModelId = carModel;
                car.LicensePlate = licensePlate;
                car.CarName = carName;
                car.UpdateAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public async Task SendRentRequestForRent(UserCar userCar)
        {
            await _context.UserCars.AddAsync(userCar);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RentConfirmDto>> GetRentRequest(int driverId)
        {

            var rentRequests = await (from uc in _context.UserCars
                                      join c in _context.Cars on uc.CarId equals c.CarId
                                      join cm in _context.CarModels on c.CarModelId equals cm.CarModelId
                                      join owner in _context.UserCars on c.CarId equals owner.CarId
                                      join ownerUser in _context.Users on owner.UserId equals ownerUser.UserId
                                      where uc.UserId == driverId && uc.Role == "Renter" // Lọc người thuê
                                            && owner.Role == "Owner" // Lọc đúng chủ xe
                                      select new RentConfirmDto
                                      {
                                          DriverId = uc.UserId,
                                          OwnerId = owner.UserId,
                                          OwnerName = ownerUser.Fullname,
                                          OwnerPhone = ownerUser.PhoneNumber,
                                          CarId = c.CarId,
                                          LicensePlate = c.LicensePlate,
                                          IsAllowedToCharge = uc.IsAllowedToCharge,
                                          Type = cm.Type,
                                          Color = cm.Color,
                                          Brand = cm.Brand,
                                          StartDate = uc.StartDate,
                                          EndDate = uc.EndDate
                                      }).Distinct().ToListAsync();

            return rentRequests;



        }

        public async Task<UserCar?> GetUserCarAsync(int userId, int carId, string role)
        {
            return await _context.UserCars
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CarId == carId && uc.Role == role);
        }

        public async Task<bool> UpdateIsAllowedToChargeAsync(int userId, int carId, string role)
        {
            var userCar = await GetUserCarAsync(userId, carId, role);
            if (userCar == null) return false;

            userCar.IsAllowedToCharge = true;
            _context.UserCars.Update(userCar);
            await _context.SaveChangesAsync();
            return true;
        }

        public PagedResult<CarDetailDTO> GetAllCars(string? search, string? type, string? brand, bool? status, int page, int pageSize)
        {
            var query = _context.Cars
                .Where(f =>
                    (string.IsNullOrEmpty(search) || f.CarName.Contains(search) || f.LicensePlate.Contains(search)) &&
                    (string.IsNullOrEmpty(type) || f.CarModel.Type.Contains(type)) &&
                    (string.IsNullOrEmpty(brand) || f.CarModel.Brand.Contains(brand)) &&
                    (status == null || f.IsDeleted == status)
                )
                .Select(f => new CarDetailDTO
                {
                    CarId = f.CarId,
                    CarName = f.CarName,
                    LicensePlate = f.LicensePlate,
                    IsDeleted = f.IsDeleted,
                    Type = f.CarModel.Type,
                    Color = f.CarModel.Color,
                    SeatNumber = f.CarModel.SeatNumber,
                    Brand = f.CarModel.Brand,
                    BatteryCapacity = f.CarModel.BatteryCapacity,
                    MaxChargingPower = f.CarModel.MaxChargingPower,
                    AverageRange = f.CarModel.AverageRange,
                    ChargingStandard = f.CarModel.Type,
                    Img = f.CarModel.Type,
                });

            int totalCount = query.Count(); ;
            var data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<CarDetailDTO>(data, totalCount, pageSize);
        }
    }
}
