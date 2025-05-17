using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "car_model",
                columns: table => new
                {
                    car_model_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    seat_number = table.Column<int>(type: "int", nullable: true),
                    brand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    battery_capacity = table.Column<double>(type: "float", nullable: true),
                    max_charging_power = table.Column<double>(type: "float", nullable: true),
                    average_range = table.Column<double>(type: "float", nullable: true),
                    charging_standard = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    img = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__car_mode__6F9B237770240B8F", x => x.car_model_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__roles__760965CC7B4A4933", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "station_location",
                columns: table => new
                {
                    station_location_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__station___0CE32FE77AA08A18", x => x.station_location_id);
                });

            migrationBuilder.CreateTable(
                name: "car",
                columns: table => new
                {
                    car_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    car_model_id = table.Column<int>(type: "int", nullable: false),
                    car_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    license_plate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    img_front = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    img_back = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    img_frontPubblicId = table.Column<string>(type: "varchar(225)", unicode: false, maxLength: 225, nullable: true),
                    img_backPubblicId = table.Column<string>(type: "varchar(225)", unicode: false, maxLength: 225, nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__car__4C9A0DB361790547", x => x.car_id);
                    table.ForeignKey(
                        name: "FK__car__car_model_i__6B24EA82",
                        column: x => x.car_model_id,
                        principalTable: "car_model",
                        principalColumn: "car_model_id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    fullname = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    dob = table.Column<DateTime>(type: "datetime2", nullable: true),
                    gender = table.Column<bool>(type: "bit", nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__B9BE370F75EF7C0F", x => x.user_id);
                    table.ForeignKey(
                        name: "FK__users__role_id__6477ECF3",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "balance",
                columns: table => new
                {
                    balance_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    balance = table.Column<double>(type: "float", nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__balance__18188B5BD63A0434", x => x.balance_id);
                    table.ForeignKey(
                        name: "FK__balance__user_id__01142BA1",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "cccd",
                columns: table => new
                {
                    cccd_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    img_front = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    img_back = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    img_frontPubblicId = table.Column<string>(type: "varchar(225)", unicode: false, maxLength: 225, nullable: true),
                    img_backPubblicId = table.Column<string>(type: "varchar(225)", unicode: false, maxLength: 225, nullable: true),
                    code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__cccd__E47A6DF186DFDDA1", x => x.cccd_id);
                    table.ForeignKey(
                        name: "FK__cccd__user_id__0F624AF8",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "charging_station",
                columns: table => new
                {
                    station_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    station_location_id = table.Column<int>(type: "int", nullable: false),
                    station_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxConsumPower = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__charging__44B370E943FB6D23", x => x.station_id);
                    table.ForeignKey(
                        name: "FK__charging___owner__6754599E",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK__charging___stati__68487DD7",
                        column: x => x.station_location_id,
                        principalTable: "station_location",
                        principalColumn: "station_location_id");
                });

            migrationBuilder.CreateTable(
                name: "driver_license",
                columns: table => new
                {
                    driver_license_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    img_front = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    img_back = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    img_frontPubblicId = table.Column<string>(type: "varchar(225)", unicode: false, maxLength: 225, nullable: true),
                    img_backPubblicId = table.Column<string>(type: "varchar(225)", unicode: false, maxLength: 225, nullable: true),
                    code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    @class = table.Column<string>(name: "class", type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__driver_l__5EB6C89FF2E32C21", x => x.driver_license_id);
                    table.ForeignKey(
                        name: "FK__driver_li__user___123EB7A3",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    token_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    revoked = table.Column<bool>(type: "bit", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__refresh___CB3C9E1740EDF086", x => x.token_id);
                    table.ForeignKey(
                        name: "FK__refresh_t__user___06CD04F7",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "user_car",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    car_id = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsAllowedToCharge = table.Column<bool>(type: "bit", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_car__9D7797D4EF334824", x => new { x.user_id, x.car_id });
                    table.ForeignKey(
                        name: "FK__user_car__car_id__71D1E811",
                        column: x => x.car_id,
                        principalTable: "car",
                        principalColumn: "car_id");
                    table.ForeignKey(
                        name: "FK__user_car__user_i__70DDC3D8",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "balance_transactions",
                columns: table => new
                {
                    transaction_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    balance_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<double>(type: "float", nullable: true),
                    order_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    transaction_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    transaction_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__balance___85C600AFE67830BB", x => x.transaction_id);
                    table.ForeignKey(
                        name: "FK__balance_t__balan__03F0984C",
                        column: x => x.balance_id,
                        principalTable: "balance",
                        principalColumn: "balance_id");
                });

            migrationBuilder.CreateTable(
                name: "charging_point",
                columns: table => new
                {
                    charging_point_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    station_id = table.Column<int>(type: "int", nullable: false),
                    charging_point_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    max_power = table.Column<double>(type: "float", nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxConsumPower = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__charging__D7F59537E93399D3", x => x.charging_point_id);
                    table.ForeignKey(
                        name: "FK__charging___stati__6E01572D",
                        column: x => x.station_id,
                        principalTable: "charging_station",
                        principalColumn: "station_id");
                });

            migrationBuilder.CreateTable(
                name: "document_review",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    cccd_id = table.Column<int>(type: "int", nullable: true),
                    driver_license_id = table.Column<int>(type: "int", nullable: true),
                    car_id = table.Column<int>(type: "int", nullable: true),
                    review_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    reviewed_by = table.Column<int>(type: "int", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__document__60883D90C6213A68", x => x.review_id);
                    table.ForeignKey(
                        name: "FK__document___car_i__1AD3FDA4",
                        column: x => x.car_id,
                        principalTable: "car",
                        principalColumn: "car_id");
                    table.ForeignKey(
                        name: "FK__document___cccd___19DFD96B",
                        column: x => x.cccd_id,
                        principalTable: "cccd",
                        principalColumn: "cccd_id");
                    table.ForeignKey(
                        name: "FK__document___drive__1BC821DD",
                        column: x => x.driver_license_id,
                        principalTable: "driver_license",
                        principalColumn: "driver_license_id");
                    table.ForeignKey(
                        name: "FK__document___revie__1CBC4616",
                        column: x => x.reviewed_by,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK__document___user___18EBB532",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "charging_session",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    car_id = table.Column<int>(type: "int", nullable: false),
                    charging_point_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    energy_consumed = table.Column<double>(type: "float", nullable: true),
                    cost = table.Column<double>(type: "float", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__charging__69B13FDCFCA28892", x => x.session_id);
                    table.ForeignKey(
                        name: "FK__charging___car_i__787EE5A0",
                        column: x => x.car_id,
                        principalTable: "car",
                        principalColumn: "car_id");
                    table.ForeignKey(
                        name: "FK__charging___charg__797309D9",
                        column: x => x.charging_point_id,
                        principalTable: "charging_point",
                        principalColumn: "charging_point_id");
                    table.ForeignKey(
                        name: "FK__charging___user___7A672E12",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "feedback",
                columns: table => new
                {
                    feedback_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    car_id = table.Column<int>(type: "int", nullable: true),
                    station_id = table.Column<int>(type: "int", nullable: true),
                    point_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    response = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__feedback__7A6B2B8C73826BE4", x => x.feedback_id);
                    table.ForeignKey(
                        name: "FK__feedback__car_id__09A971A2",
                        column: x => x.car_id,
                        principalTable: "car",
                        principalColumn: "car_id");
                    table.ForeignKey(
                        name: "FK__feedback__point___0B91BA14",
                        column: x => x.point_id,
                        principalTable: "charging_point",
                        principalColumn: "charging_point_id");
                    table.ForeignKey(
                        name: "FK__feedback__statio__0C85DE4D",
                        column: x => x.station_id,
                        principalTable: "charging_station",
                        principalColumn: "station_id");
                    table.ForeignKey(
                        name: "FK__feedback__user_i__0A9D95DB",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "real_time_data",
                columns: table => new
                {
                    data_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chargingpoint_id = table.Column<int>(type: "int", nullable: false),
                    car_id = table.Column<int>(type: "int", nullable: false),
                    license_plate = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    battery_level = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    battery_voltage = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    charging_current = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    temperature = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    charging_power = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    powerpoint = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    charging_time = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    energy_consumed = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    step_cost = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    cost = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    time_moment = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__real_tim__F5A76B3BC73E5A4C", x => x.data_id);
                    table.ForeignKey(
                        name: "FK__real_time__car_i__74AE54BC",
                        column: x => x.car_id,
                        principalTable: "car",
                        principalColumn: "car_id");
                    table.ForeignKey(
                        name: "FK__real_time__charg__75A278F5",
                        column: x => x.chargingpoint_id,
                        principalTable: "charging_point",
                        principalColumn: "charging_point_id");
                });

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    session_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<double>(type: "float", nullable: true),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    payment_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    payment_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__payment__ED1FC9EA073110F9", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK__payment__session__7E37BEF6",
                        column: x => x.session_id,
                        principalTable: "charging_session",
                        principalColumn: "session_id");
                    table.ForeignKey(
                        name: "FK__payment__user_id__7D439ABD",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_balance_user_id",
                table: "balance",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_balance_transactions_balance_id",
                table: "balance_transactions",
                column: "balance_id");

            migrationBuilder.CreateIndex(
                name: "IX_car_car_model_id",
                table: "car",
                column: "car_model_id");

            migrationBuilder.CreateIndex(
                name: "IX_cccd_user_id",
                table: "cccd",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_charging_point_station_id",
                table: "charging_point",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_charging_session_car_id",
                table: "charging_session",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_charging_session_charging_point_id",
                table: "charging_session",
                column: "charging_point_id");

            migrationBuilder.CreateIndex(
                name: "IX_charging_session_user_id",
                table: "charging_session",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_charging_station_owner_id",
                table: "charging_station",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_charging_station_station_location_id",
                table: "charging_station",
                column: "station_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_review_car_id",
                table: "document_review",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_review_cccd_id",
                table: "document_review",
                column: "cccd_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_review_driver_license_id",
                table: "document_review",
                column: "driver_license_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_review_reviewed_by",
                table: "document_review",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_document_review_user_id",
                table: "document_review",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_driver_license_user_id",
                table: "driver_license",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_car_id",
                table: "feedback",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_point_id",
                table: "feedback",
                column: "point_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_station_id",
                table: "feedback",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_user_id",
                table: "feedback",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_session_id",
                table: "payment",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_user_id",
                table: "payment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_real_time_data_car_id",
                table: "real_time_data",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_real_time_data_chargingpoint_id",
                table: "real_time_data",
                column: "chargingpoint_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_car_car_id",
                table: "user_car",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "UQ__users__A1936A6B3D8FA6CF",
                table: "users",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__users__AB6E616458702F6A",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "balance_transactions");

            migrationBuilder.DropTable(
                name: "document_review");

            migrationBuilder.DropTable(
                name: "feedback");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropTable(
                name: "real_time_data");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_car");

            migrationBuilder.DropTable(
                name: "balance");

            migrationBuilder.DropTable(
                name: "cccd");

            migrationBuilder.DropTable(
                name: "driver_license");

            migrationBuilder.DropTable(
                name: "charging_session");

            migrationBuilder.DropTable(
                name: "car");

            migrationBuilder.DropTable(
                name: "charging_point");

            migrationBuilder.DropTable(
                name: "car_model");

            migrationBuilder.DropTable(
                name: "charging_station");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "station_location");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
