using DataAccess.DTOs.ChargingStation;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;

namespace DataAccess.Interfaces
{
    public interface IChargingPointRepository
    {
        PagedResult<ChargingPointDto>? GetAllPointsByStation(int stationId, int page, int pageSize);
        ChargingPointDto GetPointById(int pointId);
        Task AddChargingPoints(List<ChargingPoint> points);
        Task<ChargingPoint?> UpdateChargingPoint(int pointId, UpdateChargingPointDto pointDto);
        Task<bool> DeleteChargingPoint(int pointId);
    }
}
