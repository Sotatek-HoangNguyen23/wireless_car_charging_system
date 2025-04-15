using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class DriverLicenseRepository : IDriverLicenseRepository
    {
        private readonly WccsContext _context;
        public DriverLicenseRepository(WccsContext context)
        {
            _context = context;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<PagedResultD<DriverLicenseDTO>> GetPagedLicensesAsync(int pageNumber, int pageSize, DriverLicenseFilter filter)
        {
            var query = _context.DriverLicenses
                .Include(dl => dl.User)
                .AsQueryable();

            // Apply filters (unchanged)
            if (!string.IsNullOrEmpty(filter.Code))
                query = query.Where(dl => dl.Code != null && dl.Code.Contains(filter.Code));
            if (!string.IsNullOrEmpty(filter.Fullname))
                query = query.Where(dl => dl.User != null && dl.User.Fullname!.Contains(filter.Fullname));
            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(dl => dl.Status == filter.Status);
            if (!string.IsNullOrEmpty(filter.Class))
                query = query.Where(dl => dl.Class == filter.Class);
            if (filter.FromCreateDate.HasValue)
                query = query.Where(dl => dl.CreateAt >= filter.FromCreateDate);
            if (filter.ToCreateDate.HasValue)
                query = query.Where(dl => dl.CreateAt <= filter.ToCreateDate);
            if (filter.FromUpdateDate.HasValue)
                query = query.Where(dl => dl.UpdateAt >= filter.FromUpdateDate);
            if (filter.ToUpdateDate.HasValue)
                query = query.Where(dl => dl.UpdateAt <= filter.ToUpdateDate);

            var totalCount = await query.CountAsync();
            var items = await query
                   .OrderByDescending(dl => dl.CreateAt)
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .Select(dl => new DriverLicenseDTO
                   {
                       LicenseId = dl.DriverLicenseId,
                       LicenseNumber = dl.Code ?? "N/A",
                       Class = dl.Class ?? "N/A",
                       FrontImageUrl = dl.ImgFront ?? "N/A",
                       BackImageUrl = dl.ImgBack ?? "N/A",
                       Status = dl.Status ?? "N/A",
                       CreatedAt = dl.CreateAt,
                       UpdatedAt = dl.UpdateAt,
                       User = new UserSimpleDTO
                       {
                           UserId = dl.User.UserId,
                           Fullname = dl.User.Fullname ?? "",
                           Email = dl.User.Email,
                           PhoneNumber = dl.User.PhoneNumber
                       }
                   })
                   .AsNoTracking()
                   .ToListAsync();


            return new PagedResultD<DriverLicenseDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        public async Task DeleteLicense(string licenseId)
        {
            if (string.IsNullOrWhiteSpace(licenseId))
            {
                throw new ArgumentException("LicenseId không hợp lệ", nameof(licenseId));
            }

            if (!int.TryParse(licenseId, out int id))
            {
                throw new ArgumentException("LicenseId phải là số nguyên", nameof(licenseId));
            }

            var license = await _context.DriverLicenses.FindAsync(id);

            if (license == null)
            {
                throw new ArgumentException("Không tìm thấy bằng lái với ID đã cho");
            }

            _context.DriverLicenses.Remove(license);
            await _context.SaveChangesAsync();
        }


        public async Task<DriverLicense?> GetLicenseByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Mã số bằng lái không hợp lệ", nameof(code));
            }

            return await _context.DriverLicenses.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code.Trim());
        }

        public async Task SaveLicense(DriverLicense liscense)
        {
            if (liscense == null)
            {
                throw new ArgumentNullException(nameof(liscense));
            }
            var existingLicense = await _context.DriverLicenses
                .FirstOrDefaultAsync(dl => dl.Code == liscense.Code);
            if (existingLicense != null)
            {
                throw new ArgumentException("Mã số bằng lái đã tồn tại", nameof(liscense.Code));
            }
                await _context.DriverLicenses.AddAsync(liscense);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLicense(DriverLicense license)
        {
            if (license == null)
            {
                throw new ArgumentNullException(nameof(license));
            }

            var existingLicense = await _context.DriverLicenses
                .FirstOrDefaultAsync(dl => dl.DriverLicenseId == license.DriverLicenseId);

            if (existingLicense == null)
            {
                throw new KeyNotFoundException("Không tìm thấy bằng lái để cập nhật");
            }
            if (existingLicense.Code != license.Code)
            {
                var existingCode = await _context.DriverLicenses
                    .FirstOrDefaultAsync(dl => dl.Code == license.Code);
                if (existingCode != null)
                {
                    throw new ArgumentException("Mã số bằng lái đã tồn tại", nameof(license.Code));
                }
            }

            _context.Entry(existingLicense).CurrentValues.SetValues(license);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DriverLicense>?> GetLicensesByUserId(int userId)
        {
            return await _context.DriverLicenses
                .Include(dl => dl.User)
                .AsNoTracking()
                .Where(dl => dl.UserId == userId)
                .ToListAsync();
        }
    }
}
