using DataAccess.Repositories.StationRepo;

namespace DataAccess.DTOs.ChargingStation
{
    public class ChargingStationDto
    {
        public int StationId { get; set; }
        public int OwnerId { get; set; }
        public string? Owner { get; set; }
        public string? StationName { get; set; }
        public string? Status { get; set; }
        public string? Address { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public double? Distance { get; set; }
        public int TotalPoint { get; set; }
        public int AvailablePoint { get; set; }
        public string? LocationDescription { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public double? MaxConsumPower { get; set; }
    }

    public class StationDetailDto
    {
        public ChargingStationDto Station { get; set; }
        public PagedResult<ChargingPointDto>? Points { get; set; }
    }

    public class NewChargingStationDto
    {
        public int OwnerId { get; set; }
        public string? StationName { get; set; }
        public string? Address { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public int TotalPoint { get; set; }
        public string? PointDescription { get; set; }
        public string? LocationDescription { get; set; }
        public string PointCode { get; set; }
        public double? MaxPower { get; set; }
    }

    public class UpdateChargingStationDto
    {
        public string? StationName { get; set; }
        public string? Status { get; set; }
        public double? MaxConsumPower { get; set; }
    }

    public class ChargingStationStatsDto
    {
        public double TotalEnergyConsumed { get; set; } // Tổng năng lượng tiêu thụ (kWh)
        public double TotalRevenue { get; set; } // Tổng doanh thu (VNĐ)
        public int TotalChargingSessions { get; set; } // Tổng số lượt sạc
        public double AverageChargingTime { get; set; } // Thời gian trung bình mỗi lượt sạc (phút)

        public List<ChartDataDto> ChartData { get; set; } = new List<ChartDataDto>(); // Dữ liệu biểu đồ
    }

    public class ChartDataDto
    {
        public string Label { get; set; } // Nhãn (ngày hoặc tháng)
        public double Revenue { get; set; } // Doanh thu (VNĐ)
        public int SessionCount { get; set; } // Số lượt sạc
    }


}
