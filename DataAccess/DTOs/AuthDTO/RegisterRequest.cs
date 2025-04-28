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
        public required string Email { get; set; }

        public required string PhoneNumber { get; set; }
        public required DateTime Dob { get; set; }
        public required bool Gender { get; set; }
        public required string Address { get; set; }

        [StringLength(12, MinimumLength = 9)]
        public required string CccdCode { get; set; }
        public required string PasswordHash { get; set; }

        public required IFormFile CCCDFrontImage { get; set; }

        public required IFormFile CCCDBackImage { get; set; }
    }
}
