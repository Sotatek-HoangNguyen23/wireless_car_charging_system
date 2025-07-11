﻿using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.StationRepo
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
                .Include(cp => cp.RealTimeData)            
                .Where(cp => cp.StationId == stationId)
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

            return new PagedResult<ChargingPointDto>(data, totalRecords, pageSize);
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
                throw new ArgumentException("Point list is empty!");

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

        public async Task<ChargingPoint?> DeleteChargingPoint(int pointId)
        {
            var point = await _context.ChargingPoints
                .FirstOrDefaultAsync(p => p.ChargingPointId == pointId);

            if (point == null)
                return null;

            point.Status = "Deleted";

            await _context.SaveChangesAsync();

            return point;
        }

    }
}
