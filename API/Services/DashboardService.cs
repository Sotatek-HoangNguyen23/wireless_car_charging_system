using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using System.Globalization;

namespace API.Services
{
    public class DashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<SystemOverviewDto> GetSystemOverviewAsync()
        {
            return await _dashboardRepository.GetSystemOverviewAsync();
        }

        public async Task<ChargingSessionStatDto> GetSessionStatistics(FilterDto filter, int userId, string role)
        {
            var sessions = await _dashboardRepository.GetSessionsAsync(filter, userId, role);

            var daily = sessions
                .GroupBy(s => s.StartTime.Value.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DailyStat
                {
                    Date = g.Key,
                    SessionCount = g.Count()
                }).ToList();

            var weekly = sessions
                .GroupBy(s => new { Year = s.StartTime.Value.Year, Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(s.StartTime.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday) })
                .Select(g => new WeeklyStat
                {
                    Year = g.Key.Year,
                    WeekNumber = g.Key.Week,
                    SessionCount = g.Count()
                })
                .OrderBy(w => w.Year).ThenBy(w => w.WeekNumber)
                .ToList();

            var monthly = sessions
                .GroupBy(s => new { s.StartTime.Value.Year, s.StartTime.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyStat
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    SessionCount = g.Count()
                }).ToList();

            int totalSession = sessions.Count();
            double avgDuration = sessions.Any() ? sessions.Average(s => (s.EndTime - s.StartTime)?.TotalMinutes ?? 0) : 0;
            double totalEnergy = sessions.Sum(s => s.EnergyConsumed ?? 0);
            int failedCount = sessions.Count(s => s.Status == "Failed");

            return new ChargingSessionStatDto
            {
                Daily = daily,
                Weekly = weekly,
                Monthly = monthly,
                TotalSession = totalSession,
                AvgDuration = avgDuration,
                TotalEnergy = totalEnergy,
                FailedSessions = failedCount
            };
        }

        public async Task<RevenueStatsDto> GetRevenueStatistics(FilterDto filter, int userId, string role)
        {
            var sessions = await _dashboardRepository.GetSessionsAsync(filter, userId, role);
            var validSessions = sessions
                .Where(s => s.Cost.HasValue && s.Cost > 0 && s.StartTime.HasValue)
                .ToList();

            var total = validSessions.Sum(s => s.Cost ?? 0);
            var avg = validSessions.Any() ? validSessions.Average(s => s.Cost ?? 0) : 0;

            var daily = validSessions
                .GroupBy(s => s.StartTime!.Value.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DailyRevenueDto { Date = g.Key, Revenue = g.Sum(s => s.Cost ?? 0) })
                .ToList();

            var monthly = validSessions
                .GroupBy(s => new { s.StartTime!.Value.Year, s.StartTime.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyRevenueDto { Year = g.Key.Year, Month = g.Key.Month, Revenue = g.Sum(s => s.Cost ?? 0) })
                .ToList();

            var yearly = validSessions
                .GroupBy(s => s.StartTime!.Value.Year)
                .OrderBy(g => g.Key)
                .Select(g => new YearlyRevenueDto { Year = g.Key, Revenue = g.Sum(s => s.Cost ?? 0) })
                .ToList();

            var byStation = validSessions
                .Where(s => s.ChargingPoint?.Station != null)
                .GroupBy(s => new { s.ChargingPoint.Station.StationId, s.ChargingPoint.Station.StationName })
                .Select(g => new StationRevenueDto { StationId = g.Key.StationId, StationName = g.Key.StationName, Revenue = g.Sum(s => s.Cost ?? 0) })
                .OrderByDescending(s => s.Revenue)
                .ToList();

            return new RevenueStatsDto
            {
                TotalRevenue = total,
                AvgRevenuePerSession = avg,
                Daily = daily,
                Monthly = monthly,
                Yearly = yearly,
                ByStation = byStation
            };
        }

        public UserStatisticsDto GetUserStatistics(FilterDto filter)
        {
            return _dashboardRepository.GetStatistics(filter);
        }

    }

}

