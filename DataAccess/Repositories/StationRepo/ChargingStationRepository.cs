using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DataAccess.Repositories.StationRepo
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }
        public int TotalPages { get; set; }

        public PagedResult(List<T> data, int totalRecords, int pageSize)
        {
            Data = data;
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        }
    }

    public class ChargingStationRepository : IChargingStationRepository
    {
        private readonly WccsContext _context;

        public ChargingStationRepository(WccsContext context)
        {
            _context = context;
        }

        public PagedResult<ChargingStationDto> GetAllStation(string? keyword, decimal? userLat, decimal? userLng, int page, int pageSize)
        {
            // Lấy tất cả dữ liệu Stationn
            var query = _context.ChargingStations
                .Include(cs => cs.StationLocation)
                .Include(cs => cs.ChargingPoints)
                .AsQueryable();

            // Tìm kiếm theo từ khóa (name hoặc location)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.ToLower();
                query = query.Where(cs =>
                    !string.IsNullOrEmpty(cs.StationName) && cs.StationName.ToLower().Contains(lowerKeyword) ||
                    cs.StationLocation != null && cs.StationLocation.Address.ToLower().Contains(lowerKeyword)
                );
            }

            // Chuyển dữ liệu sang DTO
            var stationList = query.AsNoTracking()
                            .Select(cs => new ChargingStationDto
                            {
                                StationId = cs.StationId,
                                OwnerId = cs.OwnerId,
                                Owner = cs.Owner.Fullname,
                                StationName = cs.StationName,
                                Status = cs.Status,
                                Address = cs.StationLocation.Address,
                                Longitude = cs.StationLocation.Longitude,
                                Latitude = cs.StationLocation.Latitude,
                                TotalPoint = cs.ChargingPoints.Count(),
                                AvailablePoint = cs.ChargingPoints.Count(cp => cp.Status == "Available"),
                                CreateAt = cs.CreateAt,
                                UpdateAt = cs.UpdateAt,
                                MaxConsumPower = cs.MaxConsumPower
                            })
                            .ToList();

            // Tính khoảng cách nếu có thông tin vị trí người dùng
            if (userLat.HasValue && userLng.HasValue)
            {
                foreach (var station in stationList)
                {
                    station.Distance = GetDistance(userLat.Value, userLng.Value, station.Latitude, station.Longitude);
                }

                // Sắp xếp theo khoảng cách (gần nhất trước)
                stationList = stationList.OrderBy(s => s.Distance).ToList();
            }

            // Tính tổng số trang
            int totalRecords = query.Count(); ; ;    

            // Phân trang (chỉ lấy dữ liệu của trang hiện tại)
            var data = stationList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<ChargingStationDto> (data, totalRecords, pageSize);
        }

        // Hàm tính khoảng cách giữa hai điểm theo công thức Haversine
        public double GetDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371; // Bán kính Trái Đất (km)
            double dLat = (double)(lat2 - lat1) * Math.PI / 180;
            double dLon = (double)(lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos((double)lat1 * Math.PI / 180) * Math.Cos((double)lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Khoảng cách (km)
        }

        public ChargingStationDto? GetStationById(int stationId)
        {
            var station = _context.ChargingStations
                .Include(cs => cs.StationLocation)
                .Include(cs => cs.ChargingPoints)           // Get Station + Location & Point
                .Where(cs => cs.StationId == stationId)     // Compare StationID with variable
                .AsNoTracking()
                .Select(cs => new ChargingStationDto
                {
                    StationId = cs.StationId,
                    OwnerId = cs.OwnerId,
                    Owner = cs.Owner.Fullname,
                    StationName = cs.StationName,
                    Status = cs.Status,
                    Address = cs.StationLocation.Address,
                    Longitude = cs.StationLocation.Longitude,
                    Latitude = cs.StationLocation.Latitude,
                    TotalPoint = cs.ChargingPoints.Count(),
                    AvailablePoint = cs.ChargingPoints.Count(cp => cp.Status == "Available"),
                    LocationDescription = cs.StationLocation.Description,
                    CreateAt = cs.CreateAt,
                    UpdateAt = cs.UpdateAt,
                    MaxConsumPower = cs.MaxConsumPower
                })
                .FirstOrDefault(); // Get first object

            return station;
        }

        public async Task<ChargingStation> AddChargingStation(ChargingStation station)
        {
            _context.ChargingStations.Add(station);
            await _context.SaveChangesAsync();

            return await _context.ChargingStations
                .Include(cs => cs.Owner)
                .Include(cs => cs.StationLocation)
                .Include(cs => cs.ChargingPoints)
                .FirstOrDefaultAsync(cs => cs.StationId == station.StationId);
        }

        public async Task<ChargingStation?> UpdateChargingStation(int stationId, UpdateChargingStationDto stationDto)
        {
            var station = await _context.ChargingStations
                .Include(cs => cs.Owner)
                .Include(cs => cs.StationLocation)
                .Include(cs => cs.ChargingPoints)
                .FirstOrDefaultAsync(s => s.StationId == stationId);

            if (station == null)
                return null;

            // Chỉ cập nhật nếu DTO có giá trị (tránh ghi đè null)
            if (!string.IsNullOrEmpty(stationDto.StationName)) station.StationName = stationDto.StationName;
            if (stationDto.OwnerId != 0)
            {
                station.OwnerId = stationDto.OwnerId;
            }
            if (!string.IsNullOrEmpty(stationDto.Status)) station.Status = stationDto.Status;
            if (stationDto.MaxConsumPower.HasValue) station.MaxConsumPower = stationDto.MaxConsumPower;

            if (stationDto.Latitude != 0 || stationDto.Longitude != 0 || !stationDto.Address.IsNullOrEmpty())
            {
                if (station.StationLocation == null)
                {
                    station.StationLocation = new StationLocation(); // Khởi tạo nếu null
                }

                if (stationDto.Latitude != 0) station.StationLocation.Latitude = stationDto.Latitude;
                if (stationDto.Longitude != 0) station.StationLocation.Longitude = stationDto.Longitude;
                if (!string.IsNullOrEmpty(stationDto.Address)) station.StationLocation.Address = stationDto.Address;
            }

            station.UpdateAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return await _context.ChargingStations
                .Include(cs => cs.Owner)
                .Include(cs => cs.StationLocation)
                .Include(cs => cs.ChargingPoints)
                .FirstOrDefaultAsync(cs => cs.StationId == station.StationId);
        }


        public async Task<bool> DeleteChargingStation(int stationId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Xóa ChargingPoints bằng SQL để giảm tải bộ nhớ
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM charging_point WHERE station_id = {0}", stationId);

            // Tìm station để xóa bằng EF → để vẫn giữ được tracking nếu sau này muốn extend logic
            var station = await _context.ChargingStations
                .FirstOrDefaultAsync(s => s.StationId == stationId);

            if (station == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            _context.ChargingStations.Remove(station);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }


        public List<ChargingSession> GetSessionByStation(int stationId)
        {
            return _context.ChargingSessions
                .Where(s => s.ChargingPoint.StationId == stationId)
                .ToList();
        }

        public async Task<StationLocation> AddStationLocation(StationLocation location)
        {
            _context.StationLocations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }
    }
}
