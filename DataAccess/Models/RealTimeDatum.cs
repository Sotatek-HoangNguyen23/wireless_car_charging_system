﻿using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class RealTimeDatum
{
    public int DataId { get; set; }

    public int ChargingpointId { get; set; }

    public int CarId { get; set; }

    public string? LicensePlate { get; set; }

    public string? BatteryLevel { get; set; }

    public string? BatteryVoltage { get; set; }

    public string? ChargingCurrent { get; set; }

    public string? Temperature { get; set; }

    public string? ChargingPower { get; set; }

    public string? Powerpoint { get; set; }

    public string? ChargingTime { get; set; }

    public string? EnergyConsumed { get; set; }

    public string? StepCost { get; set; }

    public string? Cost { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? TimeMoment { get; set; }

    public string? Status { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual ChargingPoint Chargingpoint { get; set; } = null!;
}
