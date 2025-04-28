using DataAccess.DTOs.ChargingStation;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IDashboardRepository
    {
        Task<SystemOverviewDto> GetSystemOverviewAsync();
        Task<List<ChargingSession>> GetSessionsAsync(FilterDto filter);
        UserStatisticsDto GetStatistics(FilterDto filter);
    }
}

