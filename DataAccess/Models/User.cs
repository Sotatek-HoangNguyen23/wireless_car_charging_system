﻿using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string? Fullname { get; set; }

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public DateTime? Dob { get; set; }

    public bool? Gender { get; set; }

    public string? Address { get; set; }

    public string? PasswordHash { get; set; }

    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<Balance> Balances { get; set; } = new List<Balance>();

    public virtual ICollection<Cccd> Cccds { get; set; } = new List<Cccd>();

    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();

    public virtual ICollection<ChargingStation> ChargingStations { get; set; } = new List<ChargingStation>();

    public virtual ICollection<DocumentReview> DocumentReviewReviewedByNavigations { get; set; } = new List<DocumentReview>();

    public virtual ICollection<DocumentReview> DocumentReviewUsers { get; set; } = new List<DocumentReview>();

    public virtual ICollection<DriverLicense> DriverLicenses { get; set; } = new List<DriverLicense>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UserCar> UserCars { get; set; } = new List<UserCar>();
}
