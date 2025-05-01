using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess.Models;

public partial class WccsContext : DbContext
{
    public WccsContext()
    {
    }

    public WccsContext(DbContextOptions<WccsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Balance> Balances { get; set; }

    public virtual DbSet<BalanceTransaction> BalanceTransactions { get; set; }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<CarModel> CarModels { get; set; }

    public virtual DbSet<Cccd> Cccds { get; set; }

    public virtual DbSet<ChargingPoint> ChargingPoints { get; set; }

    public virtual DbSet<ChargingSession> ChargingSessions { get; set; }

    public virtual DbSet<ChargingStation> ChargingStations { get; set; }

    public virtual DbSet<DocumentReview> DocumentReviews { get; set; }

    public virtual DbSet<DriverLicense> DriverLicenses { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<RealTimeDatum> RealTimeData { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StationLocation> StationLocations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCar> UserCars { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        if (!optionsBuilder.IsConfigured) { optionsBuilder.UseSqlServer(config.GetConnectionString("value")); }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Balance>(entity =>
        {
            entity.HasKey(e => e.BalanceId).HasName("PK__balance__18188B5B8D495C94");

            entity.ToTable("balance");

            entity.Property(e => e.BalanceId).HasColumnName("balance_id");
            entity.Property(e => e.Balance1).HasColumnName("balance");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Balances)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__balance__user_id__01142BA1");
        });

        modelBuilder.Entity<BalanceTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__balance___85C600AF5760B173");

            entity.ToTable("balance_transactions");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.BalanceId).HasColumnName("balance_id");
            entity.Property(e => e.OrderCode)
                .HasMaxLength(10)
                .HasColumnName("order_code");
            entity.Property(e => e.Status).HasMaxLength(10);
            entity.Property(e => e.TransactionDate).HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.Balance).WithMany(p => p.BalanceTransactions)
                .HasForeignKey(d => d.BalanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__balance_t__balan__03F0984C");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.CarId).HasName("PK__car__4C9A0DB316D45B2A");

            entity.ToTable("car");

            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.CarModelId).HasColumnName("car_model_id");
            entity.Property(e => e.CarName)
                .HasMaxLength(255)
                .HasColumnName("car_name");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(50)
                .HasColumnName("license_plate");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");

            entity.HasOne(d => d.CarModel).WithMany(p => p.Cars)
                .HasForeignKey(d => d.CarModelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__car__car_model_i__6B24EA82");
        });

        modelBuilder.Entity<CarModel>(entity =>
        {
            entity.HasKey(e => e.CarModelId).HasName("PK__car_mode__6F9B23777309A388");

            entity.ToTable("car_model");

            entity.Property(e => e.CarModelId).HasColumnName("car_model_id");
            entity.Property(e => e.AverageRange).HasColumnName("average_range");
            entity.Property(e => e.BatteryCapacity).HasColumnName("battery_capacity");
            entity.Property(e => e.Brand)
                .HasMaxLength(50)
                .HasColumnName("brand");
            entity.Property(e => e.ChargingStandard)
                .HasMaxLength(50)
                .HasColumnName("charging_standard");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.Img)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("img");
            entity.Property(e => e.MaxChargingPower).HasColumnName("max_charging_power");
            entity.Property(e => e.SeatNumber).HasColumnName("seat_number");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
        });

        modelBuilder.Entity<Cccd>(entity =>
        {
            entity.HasKey(e => e.CccdId).HasName("PK__cccd__E47A6DF1C9D85DB3");

            entity.ToTable("cccd");

            entity.Property(e => e.CccdId).HasColumnName("cccd_id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.ImgBack)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("img_back");
            entity.Property(e => e.ImgBackPubblicId)
                .HasMaxLength(225)
                .IsUnicode(false)
                .HasColumnName("img_backPubblicId");
            entity.Property(e => e.ImgFront)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("img_front");
            entity.Property(e => e.ImgFrontPubblicId)
                .HasMaxLength(225)
                .IsUnicode(false)
                .HasColumnName("img_frontPubblicId");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Cccds)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cccd__user_id__0F624AF8");
        });

        modelBuilder.Entity<ChargingPoint>(entity =>
        {
            entity.HasKey(e => e.ChargingPointId).HasName("PK__charging__D7F595374472C2EB");

            entity.ToTable("charging_point");

            entity.Property(e => e.ChargingPointId).HasColumnName("charging_point_id");
            entity.Property(e => e.ChargingPointName)
                .HasMaxLength(255)
                .HasColumnName("charging_point_name");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.MaxPower).HasColumnName("max_power");
            entity.Property(e => e.StationId).HasColumnName("station_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");

            entity.HasOne(d => d.Station).WithMany(p => p.ChargingPoints)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___stati__6E01572D");
        });

        modelBuilder.Entity<ChargingSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__charging__69B13FDCCEDDB129");

            entity.ToTable("charging_session");

            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.ChargingPointId).HasColumnName("charging_point_id");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.EnergyConsumed).HasColumnName("energy_consumed");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Car).WithMany(p => p.ChargingSessions)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___car_i__787EE5A0");

            entity.HasOne(d => d.ChargingPoint).WithMany(p => p.ChargingSessions)
                .HasForeignKey(d => d.ChargingPointId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___charg__797309D9");

            entity.HasOne(d => d.User).WithMany(p => p.ChargingSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___user___7A672E12");
        });

        modelBuilder.Entity<ChargingStation>(entity =>
        {
            entity.HasKey(e => e.StationId).HasName("PK__charging__44B370E91DACDA2D");

            entity.ToTable("charging_station");

            entity.Property(e => e.StationId).HasColumnName("station_id");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.StationLocationId).HasColumnName("station_location_id");
            entity.Property(e => e.StationName)
                .HasMaxLength(255)
                .HasColumnName("station_name");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");

            entity.HasOne(d => d.Owner).WithMany(p => p.ChargingStations)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___owner__6754599E");

            entity.HasOne(d => d.StationLocation).WithMany(p => p.ChargingStations)
                .HasForeignKey(d => d.StationLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___stati__68487DD7");
        });

        modelBuilder.Entity<DocumentReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__document__60883D90A1DED032");

            entity.ToTable("document_review", tb => tb.HasTrigger("trg_document_review_update"));

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.CccdId).HasColumnName("cccd_id");
            entity.Property(e => e.Comments)
                .HasMaxLength(1000)
                .HasColumnName("comments");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("create_at");
            entity.Property(e => e.DriverLicenseId).HasColumnName("driver_license_id");
            entity.Property(e => e.ReviewType)
                .HasMaxLength(20)
                .HasColumnName("review_type");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Cccd).WithMany(p => p.DocumentReviews)
                .HasForeignKey(d => d.CccdId)
                .HasConstraintName("FK__document___cccd___19DFD96B");

            entity.HasOne(d => d.DriverLicense).WithMany(p => p.DocumentReviews)
                .HasForeignKey(d => d.DriverLicenseId)
                .HasConstraintName("FK__document___drive__1AD3FDA4");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.DocumentReviewReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("FK__document___revie__1BC821DD");

            entity.HasOne(d => d.User).WithMany(p => p.DocumentReviewUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__document___user___18EBB532");
        });

        modelBuilder.Entity<DriverLicense>(entity =>
        {
            entity.HasKey(e => e.DriverLicenseId).HasName("PK__driver_l__5EB6C89FCD009AB9");

            entity.ToTable("driver_license");

            entity.Property(e => e.DriverLicenseId).HasColumnName("driver_license_id");
            entity.Property(e => e.Class)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("class");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.ImgBack)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("img_back");
            entity.Property(e => e.ImgBackPubblicId)
                .HasMaxLength(225)
                .IsUnicode(false)
                .HasColumnName("img_backPubblicId");
            entity.Property(e => e.ImgFront)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("img_front");
            entity.Property(e => e.ImgFrontPubblicId)
                .HasMaxLength(225)
                .IsUnicode(false)
                .HasColumnName("img_frontPubblicId");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.DriverLicenses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__driver_li__user___123EB7A3");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__feedback__7A6B2B8C61722772");

            entity.ToTable("feedback");

            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .HasColumnName("message");
            entity.Property(e => e.PointId).HasColumnName("point_id");
            entity.Property(e => e.Response)
                .HasMaxLength(1000)
                .HasColumnName("response");
            entity.Property(e => e.StationId).HasColumnName("station_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Car).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.CarId)
                .HasConstraintName("FK__feedback__car_id__09A971A2");

            entity.HasOne(d => d.Point).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.PointId)
                .HasConstraintName("FK__feedback__point___0B91BA14");

            entity.HasOne(d => d.Station).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.StationId)
                .HasConstraintName("FK__feedback__statio__0C85DE4D");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__feedback__user_i__0A9D95DB");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__payment__ED1FC9EA21B3B32D");

            entity.ToTable("payment");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasColumnName("payment_status");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Session).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment__session__7E37BEF6");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment__user_id__7D439ABD");
        });

        modelBuilder.Entity<RealTimeDatum>(entity =>
        {
            entity.HasKey(e => e.DataId).HasName("PK__real_tim__F5A76B3B7061A3DF");

            entity.ToTable("real_time_data");

            entity.Property(e => e.DataId).HasColumnName("data_id");
            entity.Property(e => e.BatteryLevel)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("battery_level");
            entity.Property(e => e.BatteryVoltage)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("battery_voltage");
            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.ChargingCurrent)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("charging_current");
            entity.Property(e => e.ChargingPower)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("charging_power");
            entity.Property(e => e.ChargingTime)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("charging_time");
            entity.Property(e => e.ChargingpointId).HasColumnName("chargingpoint_id");
            entity.Property(e => e.Cost)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cost");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.EnergyConsumed)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("energy_consumed");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("license_plate");
            entity.Property(e => e.Powerpoint)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("powerpoint");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Temperature)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("temperature");
            entity.Property(e => e.TimeMoment).HasColumnName("Time_moment");

            entity.HasOne(d => d.Car).WithMany(p => p.RealTimeData)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__real_time__car_i__74AE54BC");

            entity.HasOne(d => d.Chargingpoint).WithMany(p => p.RealTimeData)
                .HasForeignKey(d => d.ChargingpointId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__real_time__charg__75A278F5");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__refresh___CB3C9E1777F9F172");

            entity.ToTable("refresh_tokens");

            entity.Property(e => e.TokenId).HasColumnName("token_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.Revoked).HasColumnName("revoked");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__refresh_t__user___06CD04F7");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__roles__760965CCE087A950");

            entity.ToTable("roles");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<StationLocation>(entity =>
        {
            entity.HasKey(e => e.StationLocationId).HasName("PK__station___0CE32FE7ADEB4F5A");

            entity.ToTable("station_location");

            entity.Property(e => e.StationLocationId).HasColumnName("station_location_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitude");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370F6FFAE440");

            entity.ToTable("users");

            entity.HasIndex(e => e.PhoneNumber, "UQ__users__A1936A6B17999929").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164F258F998").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("phone_number");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__users__role_id__6477ECF3");
        });

        modelBuilder.Entity<UserCar>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.CarId }).HasName("PK__user_car__9D7797D48B7DBF91");

            entity.ToTable("user_car");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.Role).HasMaxLength(50);

            entity.HasOne(d => d.Car).WithMany(p => p.UserCars)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__user_car__car_id__71D1E811");

            entity.HasOne(d => d.User).WithMany(p => p.UserCars)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__user_car__user_i__70DDC3D8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
