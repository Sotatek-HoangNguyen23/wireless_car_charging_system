using DataAccess.DTOs.ChargingStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.CarDTO
{
    public class CarDetailDTO
    {
        public int CarId { get; set; }
        public string? CarName { get; set; }
        public string? LicensePlate { get; set; }
        public bool? IsDeleted { get; set; }
        public string? CarImgFront { get; set; }
        public string? CarImgBack { get; set; }
        public string? Owner { get; set; }
        public string? Email { get; set; }
        public int CarModelId { get; set; }
        public string? Type { get; set; }
        public string? Color { get; set; }
        public int? SeatNumber { get; set; }
        public string? Brand { get; set; }
        public double? BatteryCapacity { get; set; }
        public double? MaxChargingPower { get; set; }
        public double? AverageRange { get; set; }
        public string? ChargingStandard { get; set; }
        public string? Img { get; set; }
        public DateTime? ModelCreateAt { get; set; }
        public DateTime? ModelUpdateAt { get; set; }
        public string? OwnerName { get; set; }

        public string? OwnerAddress { get; set; }

    }

    //public class ChargingCarStatsDto
    //{
    //    public double TotalEnergyConsumed { get; set; } // Tổng năng lượng tiêu thụ (kWh)
    //    public double TotalRevenue { get; set; } // Tổng doanh thu (VNĐ)
    //    public int TotalChargingSessions { get; set; } // Tổng số lượt sạc
    //    public double AverageChargingTime { get; set; } // Thời gian trung bình mỗi lượt sạc (phút)

    //    public List<ChartDataDto> ChartData { get; set; } = new List<ChartDataDto>(); // Dữ liệu biểu đồ
    //}

    public class CarMonthlyStatDTO
    {
        public int Month { get; set; }
        public int SessionCount { get; set; }
        public double TotalCost { get; set; }
        public double TotalEnergy { get; set; }
        public double TotalTime { get; set; }
        public double AverageTime { get; set; }
    }

    public class CarFilterOptionsDto
    {
        public List<string> Brands { get; set; }
        public List<string> Types { get; set; }
    }

}

