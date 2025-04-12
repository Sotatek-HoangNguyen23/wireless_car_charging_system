using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;

namespace API.Services
{
    public class ChargingStationService
    {
        private readonly IChargingStationRepository _stationRepository;
        private readonly IChargingPointRepository _pointRepository;

        public ChargingStationService(IChargingStationRepository stationRepository, IChargingPointRepository pointRepository)
        {
            _pointRepository = pointRepository;
            _stationRepository = stationRepository;
        }

        public PagedResult<ChargingStationDto> GetChargingStations(string? keyword, decimal userLat, decimal userLng, int page, int pageSize)
        {
            return _stationRepository.GetAllStation(keyword, userLat, userLng, page, pageSize);
        }

        public StationDetailDto? GetStationDetails(int stationId, int page, int pageSize)
        {
            // Lấy thông tin trạm sạc
            var station = _stationRepository.GetStationById(stationId);

            // Nếu không tìm thấy trạm sạc, return null
            if (station == null) return null;

            // Lấy danh sách điểm sạc của trạm đó
            var points = _pointRepository.GetAllPointsByStation(stationId, page, pageSize);

            // Gộp dữ liệu vào DTO mới
            return new StationDetailDto
            {
                Station = station,
                Points = points
            };
        }

        public async Task<bool> AddChargingStation(NewChargingStationDto stationDto)
        {
            var location = new StationLocation
            {
                Address = stationDto.Address,
                Latitude = stationDto.Latitude,
                Longitude = stationDto.Longitude,
                Description = stationDto.LocationDescription,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            var savedLocation = await _stationRepository.AddStationLocation(location);

            var station = new ChargingStation
            {
                StationName = stationDto.StationName,
                OwnerId = stationDto.OwnerId,
                StationLocationId = savedLocation.StationLocationId,
                Status = "Active",
                MaxConsumPower = 0,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            var savedStation = await _stationRepository.AddChargingStation(station);

            await AddPoint(savedStation.StationId, stationDto);

            return true;
        }

        public async Task<ChargingStation?> UpdateChargingStation(int stationId, UpdateChargingStationDto stationDto)
        {
            return await _stationRepository.UpdateChargingStation(stationId, stationDto);
        }


        public async Task<bool> DeleteChargingStation(int stationId)
        {
            return await _stationRepository.DeleteChargingStation(stationId);
        }

        public ChargingPointDto? GetPointById(int pointId)
        {
            return _pointRepository.GetPointById(pointId);
        }

        public async Task<List<ChargingPoint>> AddPoint(int stationId, NewChargingStationDto stationDto)
        {
            var points = new List<ChargingPoint>();

            // Lấy danh sách điểm sạc của trạm
            var existingPoints = _pointRepository.GetAllPointsByStation(stationId, 1, int.MaxValue)?.Data ?? new List<ChargingPointDto>();

            // Xác định số thứ tự bắt đầu
            int startIndex;
            if (existingPoints.Any())
            {
                var lastPoint = existingPoints
                    .Where(p => p.ChargingPointName.StartsWith(stationDto.PointCode + "-"))
                    .Select(p => p.ChargingPointName.Split("-").Last())
                    .Where(p => int.TryParse(p, out _))
                    .Select(int.Parse)
                    .DefaultIfEmpty(0)
                    .Max();

                startIndex = lastPoint + 1;
            }
            else
            {
                startIndex = 1;
            }

            for (int i = startIndex; i < startIndex + stationDto.TotalPoint; i++)
            {
                var chargingPoint = new ChargingPoint
                {
                    StationId = stationId,
                    ChargingPointName = stationDto.PointCode + "-" + i,
                    Description = stationDto.PointDescription,
                    Status = "Available",
                    MaxPower = stationDto.MaxPower,
                    MaxConsumPower = 0,
                    CreateAt = DateTime.Now,
                    UpdateAt = DateTime.Now
                };
                points.Add(chargingPoint);
            }

            await _pointRepository.AddChargingPoints(points);
            return points;
        }


        public async Task<ChargingPoint?> UpdateChargingPoint(int pointId, UpdateChargingPointDto pointDto)
        {
            return await _pointRepository.UpdateChargingPoint(pointId, pointDto);
        }

        public async Task<bool> DeleteChargingPoint(int pointId)
        {
            return await _pointRepository.DeleteChargingPoint(pointId);
        }

        public ChargingStationStatsDto GetStats(int stationId, int? year, int? month)
        {
            var sessions = _stationRepository.GetSessionByStation(stationId);

            if (year.HasValue)
                sessions = sessions.Where(s => s.StartTime.Value.Year == year.Value).ToList();

            if (month.HasValue)
                sessions = sessions.Where(s => s.StartTime.Value.Month == month.Value).ToList();

            double totalEnergy = sessions.Sum(s => s.EnergyConsumed) ?? 0;
            double totalRevenue = sessions.Sum(s => s.Cost) ?? 0;
            int totalSessions = sessions.Count;
            double avgTime = sessions.Average(s => (s.EndTime - s.StartTime)?.TotalMinutes) ?? 0;

            var chartData = month.HasValue
                ? sessions.GroupBy(s => s.StartTime.Value.Day)
                          .Select(g => new ChartDataDto
                          {
                              Label = $"Ngày {g.Key}",
                              Revenue = Math.Round(g.Sum(s => s.Cost) ?? 0, 2),
                              SessionCount = g.Count()
                          }).ToList()
                : sessions.GroupBy(s => s.StartTime.Value.Month)
                          .Select(g => new ChartDataDto
                          {
                              Label = $"Tháng {g.Key}",
                              Revenue = Math.Round(g.Sum(s => s.Cost) ?? 0, 2),
                              SessionCount = g.Count()
                          }).ToList();

            return new ChargingStationStatsDto
            {
                TotalEnergyConsumed = Math.Round(totalEnergy, 2),
                TotalRevenue = Math.Round(totalRevenue, 2),
                TotalChargingSessions = totalSessions,
                AverageChargingTime = Math.Round(avgTime, 2),
                ChartData = chartData
            };
        }
    }
}
