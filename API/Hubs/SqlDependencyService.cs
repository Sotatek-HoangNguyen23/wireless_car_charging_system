using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Threading.Tasks;

namespace API.Hubs
{
    public class SqlDependencyService : BackgroundService
    {
        private readonly IHubContext<RealTimeHub> _hubContext;
        private readonly string _connectionString;
        private DateTime? _latestTimestamp;
        private readonly ILogger<SqlDependencyService> _logger;
        public SqlDependencyService(
        IHubContext<RealTimeHub> hubContext,
        IConfiguration configuration,
        ILogger<SqlDependencyService> logger)
        {
            _hubContext = hubContext;
            _connectionString = "server =wccsdatabase.database.windows.net; database =WCCS;uid=saodoqw;pwd=Taokobiet1@3;TrustServerCertificate=True;";
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckForChanges();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Kiểm tra mỗi 5 giây
            }
        }

        private async Task CheckForChanges()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    _logger.LogInformation("Database connection opened.");

                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT TOP 1 time_moment FROM dbo.real_time_data ORDER BY time_moment DESC", conn))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        _logger.LogInformation($"Latest timestamp from DB: {result}");

                        if (result != null && DateTime.TryParse(result.ToString(), out DateTime latest))
                        {
                            _logger.LogInformation($"Parsed latest timestamp: {latest}");
                            // ... logic cập nhật
                            _latestTimestamp = latest;
                            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "Dữ liệu bảng real_time_data đã thay đổi!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for database changes");
            }
        }
    }
}
