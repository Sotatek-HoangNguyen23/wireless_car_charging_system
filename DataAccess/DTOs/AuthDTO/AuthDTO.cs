using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataAccess.DTOs.Auth
{
    public class AuthDTO
    {
    }
    public class AuthenticateRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
    public class AuthenticateResponse
    {
        //public int Id { get; set; }
        public string? Fullname { get; set; }
        //public string? Email { get; set; }
        //public Boolean? Gender { get; set; }

        public string? Role { get; set; }
        //public string TokenType { get; set; } = "Bearer";
        public string? AccessToken { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; }
        public AuthenticateResponse(User user, string jwtToken, string refreshToken)
        {
            //Id = user.UserId;
            Fullname = user.Fullname;
            //Email = user.Email;
            //Gender= user.Gender;
            Role = user.Role?.RoleName;
            AccessToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }
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
