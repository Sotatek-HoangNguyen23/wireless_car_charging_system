using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.ChargingStation
{
    public class FilterDto
    {
        public int? StationId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

    // Overview
    public class SystemOverviewDto
    {
        public int TotalStations { get; set; }
        public int TotalChargingPoints { get; set; }
        public int TodaySessions { get; set; }
        public int WeekSessions { get; set; }
        public double TotalEnergyToday { get; set; }
        public double TotalEnergyThisMonth { get; set; }
        public double TotalRevenue { get; set; }
        public int ActiveStations { get; set; }
        public int OfflineStations { get; set; }
    }

    // Session
    public class ChargingSessionStatDto
    {
        public List<DailyStat> Daily { get; set; } = new();
        public List<WeeklyStat> Weekly { get; set; } = new();
        public List<MonthlyStat> Monthly { get; set; } = new();
        public int TotalSession {  get; set; }
        public double AvgDuration { get; set; }
        public double TotalEnergy { get; set; }
        public int FailedSessions { get; set; }
    }

    public class DailyStat
    {
        public DateTime Date { get; set; }
        public int SessionCount { get; set; }
    }

    public class WeeklyStat
    {
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public int SessionCount { get; set; }
    }

    public class MonthlyStat
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int SessionCount { get; set; }
    }

    // Revenue
    public class RevenueStatsDto
    {
        public double TotalRevenue { get; set; }
        public double AvgRevenuePerSession { get; set; }
        public List<DailyRevenueDto> Daily { get; set; } = new();
        public List<MonthlyRevenueDto> Monthly { get; set; } = new();
        public List<YearlyRevenueDto> Yearly { get; set; } = new();
        public List<StationRevenueDto> ByStation { get; set; } = new();
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public double Revenue { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public double Revenue { get; set; }
    }

    public class YearlyRevenueDto
    {
        public int Year { get; set; }
        public double Revenue { get; set; }
    }

    public class StationRevenueDto
    {
        public int StationId { get; set; }
        public string StationName { get; set; }
        public double Revenue { get; set; }
    }

    // User
    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public List<UserCountByDateDto> NewUsersOverTime { get; set; }
        public int UsersWhoCharged { get; set; }
        public int UsersWhoNeverCharged { get; set; }
        public double DAUMAUPercentage { get; set; }
        public int DAU {  get; set; }
        public int MAU {  get; set; }
    }

    public class UserCountByDateDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

}


