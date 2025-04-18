using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int UserId { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Type { get; set; }

    public int? CarId { get; set; }

    public int? StationId { get; set; }

    public int? PointId { get; set; }

    public string? Status { get; set; }

    public virtual Car? Car { get; set; }

    public virtual ChargingPoint? Point { get; set; }

    public virtual ChargingStation? Station { get; set; }

    public virtual User User { get; set; } = null!;
}
