using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.UserDTO
{
    public class DriverLicenseResponse
    {
        public required string LicenseNumber { get; set; }
        public required string Class { get; set; }
        public required string FrontImageUrl { get; set; }
        public required string BackImageUrl { get; set; }
        public required string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
