﻿using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class DriverLicense
{
    public int DriverLicenseId { get; set; }

    public int UserId { get; set; }

    public string? ImgFront { get; set; }

    public string? ImgBack { get; set; }

    public string? Code { get; set; }

    public string? Class { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual User User { get; set; } = null!;
}
