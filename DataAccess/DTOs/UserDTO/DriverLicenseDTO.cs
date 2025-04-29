using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.UserDTO
{
    public class DriverLicenseDTO
    {
        public int LicenseId { get; set; }
        public required string LicenseNumber { get; set; }
        public required string Class { get; set; }
        public required string FrontImageUrl { get; set; }
        public required string BackImageUrl { get; set; }
        public required string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public UserSimpleDTO User { get; set; } = null!;

    }
    public class UserSimpleDTO
    {
        public int UserId { get; set; }
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
    public class DriverLicenseFilter
    {
        public string? Code { get; set; }
        public string? Fullname { get; set; } // Thêm trường filter theo tên người dùng
        public string? Status { get; set; }
        public string? Class { get; set; }
        public DateTime? FromCreateDate { get; set; }
        public DateTime? ToCreateDate { get; set; }
        public DateTime? FromUpdateDate { get; set; }
        public DateTime? ToUpdateDate { get; set; }
    }
    public class PagedResultD<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
