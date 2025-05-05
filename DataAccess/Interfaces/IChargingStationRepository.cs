using DataAccess.DTOs.ChargingStation;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;

namespace DataAccess.Interfaces
{
    public interface IChargingStationRepository
    {
        PagedResult<ChargingStationDto>? GetAllStation(string? keyword, decimal? userLat, decimal? userLng, int page, int pageSize, string currentRole, int currentUserId);
        ChargingStationDto GetStationById(int stationId);      
        Task<ChargingStation> AddChargingStation(ChargingStation station);
        Task<ChargingStation?> UpdateChargingStation(int stationId, UpdateChargingStationDto stationDto);
        Task<ChargingStation?> DeleteChargingStation(int stationId);
        List<ChargingSession> GetSessionByStation(int stationId);
        Task<StationLocation> AddStationLocation(StationLocation location);
    }
}
