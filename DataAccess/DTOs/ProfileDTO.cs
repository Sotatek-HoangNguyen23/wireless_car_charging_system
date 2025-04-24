using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs
{
    public class ProfileDTO
    {
        public int UserId { get; set; }
        public string? Fullname { get; set; }

        public string Email { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public DateTime? Dob { get; set; }

        public bool? Gender { get; set; }

        public string? Address { get; set; }
        public string? Status { get; set; }

        public int CccdId { get; set; }
        public string? ImgFront { get; set; }

        public string? ImgBack { get; set; }
        public string? Code { get; set; }

    }

    public class RequestProfile
    {
        public string? Fullname { get; set; }

        public string Email { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public DateTime? Dob { get; set; }

        public bool? Gender { get; set; }

        public string? Address { get; set; }

    }

    public class ChangePassDTO
    {
       
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }


    }
}
