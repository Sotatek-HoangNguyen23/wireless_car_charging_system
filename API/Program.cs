using API.Services;
using API.Swagger;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.CarRepo;
using DataAccess.Repositories;
using DataAccess.Repositories.TestRepo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using Microsoft.AspNetCore.Http.Features;


var builder = WebApplication.CreateBuilder(args);
//=================================
// Cloudinary configuration
DotEnv.Load();
Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
cloudinary.Api.Secure = true;
builder.Services.AddSingleton(cloudinary);
// Cho phép upload file lớn (tối đa 50MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});
//=======================================
//Redis conection
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
if (string.IsNullOrEmpty(redisConnectionString))
{
    throw new ArgumentException("Redis connection string is missing");
}

var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
configurationOptions.User = "default";
configurationOptions.Password = redisPassword; 
configurationOptions.AbortOnConnectFail = false;
configurationOptions.ConnectTimeout = 15000;
configurationOptions.SyncTimeout = 15000;

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connection = ConnectionMultiplexer.Connect(configurationOptions);

    if (!connection.IsConnected)
    {
        throw new InvalidOperationException("Failed to connect to Redis");
    }

    return connection;
});
//=======================================
// Add services to the container.
builder.Services.AddDbContext<WccsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("value")), ServiceLifetime.Scoped);


builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OtpServices>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<OtpServices>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICccdRepository, CccdRepository>();
builder.Services.AddScoped<ITest, Test>();


//=======================================
// JWt configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
var key = Encoding.UTF8.GetBytes(secret!);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Kiểm tra Issuer
            ValidateAudience = true, // Kiểm tra Audience
            ValidateLifetime = true, // Kiểm tra thời hạn của token
            ValidateIssuerSigningKey = true, // Kiểm tra chữ ký và private key
            ValidIssuer = jwtSettings["Issuer"], // Cấu hình Issuer hợp lệ
            ValidAudience = jwtSettings["Audience"], // Cấu hình Audience hợp lệ
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });
// Thêm dịch vụ CORS cho phép tất cả các nguồn truy cập
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.WithOrigins("https://localhost:5216", "http://localhost:5216")
                  .AllowAnyHeader()   // Cho phép bất kỳ header nào
                  .AllowAnyMethod() // Cho phép bất kỳ method nào (GET, POST, PUT, DELETE, v.v.)
                  .AllowCredentials();
        });
});
builder.Services.AddScoped<IChargingStationRepository, ChargingStationRepository>();
builder.Services.AddScoped<ChargingStationService>();
builder.Services.AddScoped<IChargingPointRepository, ChargingPointRepository>();
builder.Services.AddScoped<IMyCars, MyCarsRepo>(); 
builder.Services.AddScoped<CarService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Operator", policy => policy.RequireRole("Operator"));
    options.AddPolicy("Driver", policy => policy.RequireRole("Driver"));
    options.AddPolicy("AdminOrOperator", policy => policy.RequireRole("Admin", "Operator"));

});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

var app = builder.Build();
app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
