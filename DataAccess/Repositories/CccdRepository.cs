using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class CccdRepository : ICccdRepository
    {
        private readonly WccsContext _context;
        public CccdRepository(WccsContext context)
        {
            _context = context;
        }
        public Task<Cccd?> GetCccdByCode(string code)
        {
            if (String.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code không thể trống hoặc khoảng trắng", nameof(code));
            }
            return _context.Cccds.Include(c => c.User)
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Code == code);
        }

        public async Task<Cccd?> GetCccdById(int id)
        {
            return await _context.Cccds
                   .Include(dl => dl.User)
                   .AsNoTracking()
                   .FirstOrDefaultAsync(dl => dl.CccdId == id);
        }

        public Task SaveCccd(Cccd cccd)
        {
            if (cccd == null)
            {
                throw new ArgumentException("Cccd không được null", nameof(cccd));
            }    
            var existingCccd = _context.Cccds
                .AsNoTracking()
                .FirstOrDefault(c => c.Code == cccd.Code);
            if (existingCccd != null) {
                throw new ArgumentException("Cccd đã tồn tại", nameof(cccd));
            }
                _context.Cccds.Add(cccd);
            return _context.SaveChangesAsync();
        }
    }
}
