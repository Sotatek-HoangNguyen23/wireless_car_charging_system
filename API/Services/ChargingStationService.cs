using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;

namespace API.Services
{
    public class ChargingStationService
    {
        private readonly IChargingStationRepository _stationRepository;
        private readonly IChargingPointRepository _pointRepository;
        private readonly IChargingLocationRepository _locationRepository;

        public ChargingStationService(IChargingStationRepository stationRepository, IChargingPointRepository pointRepository, IChargingLocationRepository locationRepository)
        {
            _pointRepository = pointRepository;
            _stationRepository = stationRepository;
            _locationRepository = locationRepository;
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
                Longitude = stationDto.Longtitude,
                Description = stationDto.LocationDescription,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            var savedLocation = await _locationRepository.AddStationLocation(location);

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
    }
}
