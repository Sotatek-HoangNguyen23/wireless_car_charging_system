﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.UserDTO
{
    public class UpdateDriverLicenseOperatorRequest
    {
        public required string NewCode { get; set; }
        public required string Fullname { get; set; }
        public required string Level { get; set; }
    }

    public class DriverLicenseRequest
    {
        public required string LicenseNumber { get; set; }
        public required string Class { get; set; }
        public required IFormFile LicenseFrontImage { get; set; }
        public required IFormFile LicenseBackImage { get; set; }
    }
    public class DriverLicenseUpdateRequest
    {
        public required string LicenseNumber { get; set; }
        public required IFormFile LicenseFrontImage { get; set; }
        public required IFormFile LicenseBackImage { get; set; }
    }
}