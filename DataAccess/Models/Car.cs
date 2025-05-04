using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Car
{
    public int CarId { get; set; }

    public int CarModelId { get; set; }

    public string? CarName { get; set; }

    public string? LicensePlate { get; set; }

    public bool? IsDeleted { get; set; }

    public string? ImgFront { get; set; }

    public string? ImgBack { get; set; }

    public string? ImgFrontPubblicId { get; set; }

    public string? ImgBackPubblicId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual CarModel CarModel { get; set; } = null!;

    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();

    public virtual ICollection<DocumentReview> DocumentReviews { get; set; } = new List<DocumentReview>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<RealTimeDatum> RealTimeData { get; set; } = new List<RealTimeDatum>();

    public virtual ICollection<UserCar> UserCars { get; set; } = new List<UserCar>();
}
