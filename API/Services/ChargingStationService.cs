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

            // Tạo danh sách ChargingPoints
            var points = new List<ChargingPoint>();
            char pointName = 'A';
            for (int i = 1; i <= stationDto.TotalPoint; i++)
            {
                var chargingPoint = new ChargingPoint
                {
                    StationId = savedStation.StationId,
                    ChargingPointName = stationDto.PointCode + "-" + (char.IsLetter(stationDto.PointName[0]) ? pointName++ : i.ToString()),
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

    }
}
