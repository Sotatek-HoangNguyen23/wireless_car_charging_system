using DataAccess.Interfaces;
using DataAccess.Models;
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


        public async Task DeleteLicense(string licenseId)
        {
            if (string.IsNullOrWhiteSpace(licenseId))
            {
                throw new ArgumentException("LicenseId không hợp lệ", nameof(licenseId));
            }

            var license = await _context.DriverLicenses
                .FirstOrDefaultAsync(dl => dl.DriverLicenseId == int.Parse(licenseId));

            if (license == null)
            {
                throw new KeyNotFoundException("Không tìm thấy bằng lái với ID đã cho");
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

            _context.Entry(existingLicense).CurrentValues.SetValues(license);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DriverLicense>?> GetLicensesByUserId(int userId)
        {
            return await _context.DriverLicenses
                .AsNoTracking()
                .Where(dl => dl.UserId == userId)
                .ToListAsync();
        }
    }
}
