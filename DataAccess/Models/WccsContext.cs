using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-3FG4MF5P;Database=WCCS;User Id=sa;Password=123456;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Balance>(entity =>
        {
            entity.HasKey(e => e.BalanceId).HasName("PK__balance__18188B5B502E8059");

            entity.ToTable("balance");

            entity.Property(e => e.BalanceId).HasColumnName("balance_id");
            entity.Property(e => e.Balance1).HasColumnName("balance");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Balances)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__balance__user_id__5BE2A6F2");
        });

        modelBuilder.Entity<BalanceTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__balance___85C600AFCA6C2E51");

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
                .HasConstraintName("FK__balance_t__balan__5EBF139D");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.CarId).HasName("PK__car__4C9A0DB3AA015144");

            entity.ToTable("car");

            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.CarModelId).HasColumnName("car_model_id");
            entity.Property(e => e.CarName)
                .HasMaxLength(255)
                .HasColumnName("car_name");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.ImgBack)
                .HasMaxLength(1000)
                .HasColumnName("img_back");
            entity.Property(e => e.ImgBackPubblicId)
                .HasMaxLength(255)
                .HasColumnName("img_backPubblicId");
            entity.Property(e => e.ImgFront)
                .HasMaxLength(1000)
                .HasColumnName("img_front");
            entity.Property(e => e.ImgFrontPubblicId)
                .HasMaxLength(255)
                .HasColumnName("img_frontPubblicId");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(50)
                .HasColumnName("license_plate");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");

            entity.HasOne(d => d.CarModel).WithMany(p => p.Cars)
                .HasForeignKey(d => d.CarModelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__car__car_model_i__45F365D3");
        });

        modelBuilder.Entity<CarModel>(entity =>
        {
            entity.HasKey(e => e.CarModelId).HasName("PK__car_mode__6F9B237727E51E14");

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
            entity.HasKey(e => e.CccdId).HasName("PK__cccd__E47A6DF1D3DF36FB");

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
                .HasConstraintName("FK__cccd__user_id__6754599E");
        });

        modelBuilder.Entity<ChargingPoint>(entity =>
        {
            entity.HasKey(e => e.ChargingPointId).HasName("PK__charging__D7F595371887C632");

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
                .HasConstraintName("FK__charging___stati__48CFD27E");
        });

        modelBuilder.Entity<ChargingSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__charging__69B13FDC2779F21B");

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
                .HasConstraintName("FK__charging___car_i__534D60F1");

            entity.HasOne(d => d.ChargingPoint).WithMany(p => p.ChargingSessions)
                .HasForeignKey(d => d.ChargingPointId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___charg__5441852A");

            entity.HasOne(d => d.User).WithMany(p => p.ChargingSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___user___5535A963");
        });

        modelBuilder.Entity<ChargingStation>(entity =>
        {
            entity.HasKey(e => e.StationId).HasName("PK__charging__44B370E9724EE29A");

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
                .HasConstraintName("FK__charging___owner__4222D4EF");

            entity.HasOne(d => d.StationLocation).WithMany(p => p.ChargingStations)
                .HasForeignKey(d => d.StationLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___stati__4316F928");
        });

        modelBuilder.Entity<DocumentReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__document__60883D9090F98612");

            entity.ToTable("document_review", tb => tb.HasTrigger("trg_document_review_update"));

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.CarId).HasColumnName("car_id");
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

            entity.HasOne(d => d.Car).WithMany(p => p.DocumentReviews)
                .HasForeignKey(d => d.CarId)
                .HasConstraintName("FK__document___car_i__4AE30379");

            entity.HasOne(d => d.Cccd).WithMany(p => p.DocumentReviews)
                .HasForeignKey(d => d.CccdId)
                .HasConstraintName("FK__document___cccd___49EEDF40");

            entity.HasOne(d => d.DriverLicense).WithMany(p => p.DocumentReviews)
                .HasForeignKey(d => d.DriverLicenseId)
                .HasConstraintName("FK__document___drive__4BD727B2");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.DocumentReviewReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("FK__document___revie__4CCB4BEB");

            entity.HasOne(d => d.User).WithMany(p => p.DocumentReviewUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__document___user___48FABB07");
        });

        modelBuilder.Entity<DriverLicense>(entity =>
        {
            entity.HasKey(e => e.DriverLicenseId).HasName("PK__driver_l__5EB6C89F740B2197");

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
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.DriverLicenses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__driver_li__user___6A30C649");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__feedback__7A6B2B8CE7D0D04C");

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
                .HasConstraintName("fk_feedback_car");

            entity.HasOne(d => d.Point).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.PointId)
                .HasConstraintName("fk_feedback_point");

            entity.HasOne(d => d.Station).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.StationId)
                .HasConstraintName("fk_feedback_station");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__feedback__user_i__6477ECF3");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__payment__ED1FC9EAE8504C78");

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
                .HasConstraintName("FK__payment__session__59063A47");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment__user_id__5812160E");
        });

        modelBuilder.Entity<RealTimeDatum>(entity =>
        {
            entity.HasKey(e => e.DataId).HasName("PK__real_tim__F5A76B3BC73E5A4C");

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
            entity.Property(e => e.StepCost)
                .HasMaxLength(50)
                .HasColumnName("step_cost");
            entity.Property(e => e.Temperature)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("temperature");
            entity.Property(e => e.TimeMoment)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("time_moment");

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
            entity.HasKey(e => e.TokenId).HasName("PK__refresh___CB3C9E175550A2B3");

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
                .HasConstraintName("FK__refresh_t__user___619B8048");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__roles__760965CCAF1A0C43");

            entity.ToTable("roles");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<StationLocation>(entity =>
        {
            entity.HasKey(e => e.StationLocationId).HasName("PK__station___0CE32FE74D6667F1");

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
            entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370FB094757A");

            entity.ToTable("users");

            entity.HasIndex(e => e.PhoneNumber, "UQ__users__A1936A6BEB9BDBF2").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164C834537B").IsUnique();

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
                .HasConstraintName("FK__users__role_id__3F466844");
        });

        modelBuilder.Entity<UserCar>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.CarId }).HasName("PK__user_car__9D7797D4BE2090A0");

            entity.ToTable("user_car");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.Role).HasMaxLength(50);

            entity.HasOne(d => d.Car).WithMany(p => p.UserCars)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__user_car__car_id__4CA06362");

            entity.HasOne(d => d.User).WithMany(p => p.UserCars)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__user_car__user_i__4BAC3F29");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
