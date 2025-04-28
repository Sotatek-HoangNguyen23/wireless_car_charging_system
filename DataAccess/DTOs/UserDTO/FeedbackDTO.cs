using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.UserDTO
{
    public class FeedbackDto
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Type { get; set; }
        public string Car { get; set; }
        public string Station { get; set; }
        public string Point { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime? Date { get; set; }
    }

    public class AddFeedbackDto
    {
        public int UserId { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; } // "Car" hoặc "Station"
        public int? CarId { get; set; }
        public int? StationId { get; set; }
        public int? PointId { get; set; }
    }

    public class UpdateFeedbackStatusDto
    {
        public string Status { get; set; } = "Processed";
    }
}
