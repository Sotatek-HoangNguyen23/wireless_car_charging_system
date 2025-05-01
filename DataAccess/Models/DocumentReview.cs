using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class DocumentReview
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int? CccdId { get; set; }

    public int? DriverLicenseId { get; set; }

    public string ReviewType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Comments { get; set; }

    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime UpdateAt { get; set; }

    public virtual Cccd? Cccd { get; set; }

    public virtual DriverLicense? DriverLicense { get; set; }

    public virtual User? ReviewedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
