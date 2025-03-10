using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Collections.Specialized.BitVector32;

namespace DataAccess.Repositories
{
    public class ChargingPointRepository : IChargingPointRepository
    {
        private readonly WccsContext _context;

        public ChargingPointRepository(WccsContext context)
        {
            _context = context;
        }

        public PagedResult<ChargingPointDto>? GetAllPointsByStation(int stationId, int page, int pageSize)
        {
            var points = _context.ChargingPoints
                .Include(cp => cp.ChargingSessions)
                .Include(cp => cp.RealTimeData)             // Get Station + Location & Point
                .Where(cp => cp.StationId == stationId)     // Compare StationID with variable
                .AsNoTracking()
                .Select(cp => new ChargingPointDto
                {
                    ChargingPointId = cp.ChargingPointId,
                    ChargingPointName = cp.ChargingPointName,
                    Description = cp.Description,
                    Status = cp.Status,
                    MaxPower = cp.MaxPower,
                    CreateAt = cp.CreateAt,
                    UpdateAt = cp.UpdateAt,
                    MaxConsumPower = cp.MaxConsumPower
                })
                .ToList();

            int totalRecords = points.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Phân trang (chỉ lấy dữ liệu của trang hiện tại)
            var data = points
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<ChargingPointDto> { Data = data, TotalPages = totalPages }; ;
        }

        public ChargingPointDto? GetPointById(int pointId)
        {
            var point = _context.ChargingPoints
                .Include(cp => cp.ChargingSessions)
                .Include(cp => cp.RealTimeData)                 // Get Station + Location & Point
                .Where(cp => cp.ChargingPointId == pointId)     // Compare ChargingPointID with variable
                .AsNoTracking()
                .Select(cp => new ChargingPointDto
                {
                    ChargingPointId = cp.ChargingPointId,
                    ChargingPointName = cp.ChargingPointName,
                    Description = cp.Description,
                    Status = cp.Status,
                    MaxPower = cp.MaxPower,
                    CreateAt = cp.CreateAt,
                    UpdateAt = cp.UpdateAt,
                    MaxConsumPower = cp.MaxConsumPower
                })
                .FirstOrDefault();

            return point;
        }

        public async Task AddChargingPoints(List<ChargingPoint> points)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("Danh sách điểm sạc trống!");

            await _context.ChargingPoints.AddRangeAsync(points);
            await _context.SaveChangesAsync();
        }

        public async Task<ChargingPoint?> UpdateChargingPoint(int pointId, UpdateChargingPointDto pointDto)
        {
            var point = await _context.ChargingPoints.FirstOrDefaultAsync(s => s.ChargingPointId == pointId);

            if (point == null)
                return null;

            // Chỉ cập nhật nếu DTO có giá trị (tránh ghi đè null)
            if (!string.IsNullOrEmpty(pointDto.ChargingPointName)) point.ChargingPointName = pointDto.ChargingPointName;
            if (!string.IsNullOrEmpty(pointDto.Description)) point.Description = pointDto.Description;
            if (!string.IsNullOrEmpty(pointDto.Status)) point.Status = pointDto.Status;
            if (pointDto.MaxConsumPower.HasValue) point.MaxConsumPower = pointDto.MaxConsumPower;
            if (pointDto.MaxPower.HasValue) point.MaxPower = pointDto.MaxPower;

            point.UpdateAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return point;
        }

        public async Task<bool> DeleteChargingPoint(int pointId)
        {
            var point = await _context.ChargingPoints                        // Load ChargingPoints để xóa
                .FirstOrDefaultAsync(s => s.ChargingPointId == pointId);

            if (point == null)
                return false;

            _context.ChargingPoints.Remove(point);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
