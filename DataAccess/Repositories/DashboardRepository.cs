using DataAccess.DTOs.ChargingStation;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly WccsContext _context;

        public DashboardRepository(WccsContext context)
        {
            _context = context;
        }

        public async Task<SystemOverviewDto> GetSystemOverviewAsync()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);

            var dto = new SystemOverviewDto
            {
                TotalStations = await _context.ChargingStations.CountAsync(),
                TotalChargingPoints = await _context.ChargingPoints.CountAsync(),
                TodaySessions = await _context.ChargingSessions.CountAsync(s => s.StartTime.HasValue && s.StartTime.Value.Date == today),
                WeekSessions = await _context.ChargingSessions.CountAsync(s => s.StartTime.HasValue && s.StartTime.Value >= startOfWeek),
                TotalEnergyToday = await _context.ChargingSessions
                    .Where(s => s.StartTime.HasValue && s.StartTime.Value.Date == today)
                    .SumAsync(s => s.EnergyConsumed ?? 0),
                TotalEnergyThisMonth = await _context.ChargingSessions
                    .Where(s => s.StartTime.HasValue && s.StartTime.Value.Month == today.Month && s.StartTime.Value.Year == today.Year)
                    .SumAsync(s => s.EnergyConsumed ?? 0),
                TotalRevenue = await _context.ChargingSessions
                    .SumAsync(s => s.Cost ?? 0),
                ActiveStations = await _context.ChargingStations.CountAsync(s => s.Status == "active"),
                OfflineStations = await _context.ChargingStations.CountAsync(s => s.Status == "offline")
            };

            return dto;
        }

        public async Task<List<ChargingSession>> GetSessionsAsync(FilterDto filter)
        {
            var query = _context.ChargingSessions
                .Include(s => s.ChargingPoint).ThenInclude(cp => cp.Station)
                .Include(s => s.Car).ThenInclude(c => c.CarModel)
                .AsQueryable();

            if (filter.StationId.HasValue)
            {
                query = query.Where(s => s.ChargingPoint != null && s.ChargingPoint.StationId == filter.StationId.Value);
            }

            if (filter.Start.HasValue)
            {
                query = query.Where(s => s.StartTime >= filter.Start.Value);
            }

            if (filter.End.HasValue)
            {
                query = query.Where(s => s.EndTime <= filter.End.Value);
            }

            return await query.ToListAsync();
        }

        public UserStatisticsDto GetStatistics(FilterDto filter)
        {
            // Lấy tất cả người dùng và phiên sạc trong khoảng thời gian xác định
            var users = _context.Users.Where(u => u.RoleId == 1).AsQueryable();

            // Tổng số người dùng đã đăng ký
            var totalUsers = users.Count();

            var sessions = _context.ChargingSessions.AsQueryable();
            if (filter.Start.HasValue)
            {
                users = users.Where(s => s.CreateAt >= filter.Start.Value);
                sessions = sessions.Where(s => s.StartTime >= filter.Start.Value);
            }

            if (filter.End.HasValue)
            {
                users = users.Where(s => s.CreateAt <= filter.End.Value);
                sessions = sessions.Where(s => s.EndTime <= filter.End.Value);
            }

            // Người dùng mới theo thời gian (phân nhóm theo ngày)
            var newUsersOverTime = users
                .Where(u => u.CreateAt.HasValue)
                .GroupBy(u => u.CreateAt.Value.Date)
                .Select(g => new UserCountByDateDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();


            // Tính số người dùng đã thực sự thực hiện sạc
            var userIdsWhoCharged = sessions.Select(s => s.UserId).Distinct();
            var usersWhoCharged = users.Count(u => userIdsWhoCharged.Contains(u.UserId));
            var usersWhoNeverCharged = totalUsers - usersWhoCharged;

            // Tính DAU (Daily Active Users) - số người dùng sạc hôm nay
            var today = DateTime.Today;
            var dau = sessions.Where(s => s.StartTime!.Value.Date == today).Select(s => s.UserId).Distinct().Count();

            // Tính MAU (Monthly Active Users) - số người dùng sạc trong tháng này
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var mau = sessions.Where(s => s.StartTime!.Value.Date >= startOfMonth && s.StartTime!.Value.Date <= today)
                              .Select(s => s.UserId).Distinct().Count();

            // Tính tỷ lệ DAU/MAU
            double dauMauRatio = mau > 0 ? Math.Round((double)dau / mau * 100, 2) : 0;

            // Trả về DTO chứa các thông tin thống kê
            return new UserStatisticsDto
            {
                TotalUsers = totalUsers,
                NewUsersOverTime = newUsersOverTime,
                UsersWhoCharged = usersWhoCharged,
                UsersWhoNeverCharged = usersWhoNeverCharged,
                DAU = dau,
                MAU = mau,
                DAUMAUPercentage = dauMauRatio
            };
        }

    }
}

