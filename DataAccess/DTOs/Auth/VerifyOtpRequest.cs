using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.Auth
{
    public class VerifyOtpRequest
    {
        public required string Email { get; set; }

        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số")]
        public required string OtpCode { get; set; }
    } 
    public class VerifyActiveAccountRequest
    {
        public required string Email { get; set; }

        public required string OtpCode { get; set; }
    }
}
