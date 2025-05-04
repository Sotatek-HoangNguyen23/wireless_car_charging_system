using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.CarDTO
{
    public class MyCarsDTO
    {
        public int CarId { get; set; }
        public string? CarName { get; set; }

        public string? LicensePlate { get; set; }

        public bool? IsDeleted { get; set; }
        public string? Type { get; set; }

        public string? Color { get; set; }
        public string? Brand { get; set; }
        public string? Img { get; set; }

        public string Status { get; set; }
    }

    public class AddCarRequest
    {
        public int CarModelId { get; set; }

        public string LicensePlate { get; set; }
        public string? CarName { get; set; }

        public required IFormFile CarLicenseFrontImage { get; set; }

        public required IFormFile CarLicenseBackImage { get; set; }
    }

    public class EditCarRequest
    {
        public int CarId { get; set; }
        public int CarModelId { get; set; }
        public string LicensePlate { get; set; }
        public string? CarName { get; set; }
    }

    public class RentRequestDto
    {
        public int UserId { get; set; }
        public int CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class RentConfirmDto
    {
        public int DriverId { get; set; }

        public int OwnerId { get; set; }

        public string OwnerName { get; set; }

        public string OwnerPhone { get; set; }
        public int CarId { get; set; }

        public string LicensePlate { get; set; }

        public bool? IsAllowedToCharge { get; set; }
        public string? Type { get; set; }

        public string? Color { get; set; }
        public string? Brand { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class ConfirmRentDto
    {
        public int UserId { get; set; }
        public int CarId { get; set; }
        public string Role { get; set; }
    }

    public class ChargingSessionRequest
    {
        public int CarId { get; set; }
        public int PointId { get; set; }
        public string TimeMoment { get; set; } = string.Empty;
        public string ChargingTime { get; set; } = string.Empty;
        public string Energy { get; set; } = string.Empty;
        public string Cost { get; set; } = string.Empty;
    }

}
