using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Threading.Tasks;

namespace API.Hubs
{
    public class SqlDependencyService
    {
        private readonly IHubContext<RealTimeHub> _hubContext;
        private readonly string _connectionString;

        public SqlDependencyService(IHubContext<RealTimeHub> hubContext, IConfiguration configuration)
        {
            _hubContext = hubContext;
            _connectionString = configuration.GetConnectionString("value");

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentNullException(nameof(_connectionString), "Connection string không được để trống!");
            }
        }

        public void StartListening()
        {
            try
            {
                SqlDependency.Stop(_connectionString);
                SqlDependency.Start(_connectionString);
                ListenForChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi khởi động SqlDependency: {ex.Message}");
            }
        }

        private void ListenForChanges()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT data_id, car_id, chargingpoint_id, battery_level, charging_power, temperature, timestamp FROM dbo.real_time_data", connection))
                {
                    SqlDependency dependency = new SqlDependency(command);
                    dependency.OnChange += OnDatabaseChange;
                    command.ExecuteReader();  // Chạy query để kích hoạt SqlDependency
                }
            }
        }

        private async void OnDatabaseChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "Dữ liệu bảng real_time_data đã thay đổi!");
                ListenForChanges(); // Đăng ký lại để tiếp tục lắng nghe
            }
        }
    }
}
