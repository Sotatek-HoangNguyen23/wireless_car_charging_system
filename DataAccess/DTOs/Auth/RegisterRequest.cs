using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; 

namespace DataAccess.DTOs.Auth
{
    public class RegisterRequest
    {
        public required string Fullname { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public required string Email { get; set; }

        public required string PhoneNumber { get; set; }
        public required DateTime Dob { get; set; }
        public required bool Gender { get; set; }

        [Required(ErrorMessage = "Mã CCCD là bắt buộc")]
        [StringLength(12, MinimumLength = 9)]
        public required string CccdCode { get; set; }
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public required string PasswordHash { get; set; }

        [Required(ErrorMessage = "Ảnh mặt trước CCCD là bắt buộc")]
        public required IFormFile CCCDFrontImage { get; set; }

        [Required(ErrorMessage = "Ảnh mặt sau CCCD là bắt buộc")]
        public required IFormFile CCCDBackImage { get; set; }
    }
}
