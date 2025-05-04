using DataAccess.DTOs.UserDTO;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IDriverLicenseRepository
    {
        Task<DriverLicense?> GetLicenseByCode(string code);
        Task SaveLicense(DriverLicense liscense);
        Task DeleteLicense(string licenseId);
        Task UpdateLicense(DriverLicense liscense);
        Task<IEnumerable<DriverLicense>?> GetLicensesByUserId(int userId);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<PagedResultD<DriverLicenseDTO>> GetPagedLicensesAsync(int pageNumber, int pageSize, DriverLicenseFilter filter);
        Task<DriverLicense?> GetLicensesById(int id);
        Task ChangeLicenseStatusAsync(int? licenseId, string newStatus);
    }
}
