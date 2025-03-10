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
                throw new ArgumentException("Code cannot be blank or contain only spaces", nameof(code));
            }
            return _context.Cccds.Include(c => c.User)
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Code == code);
        }

        public Task SaveCccd(Cccd cccd)
        {
            if (cccd == null)
            {
                throw new ArgumentException("Cccd cannot be null", nameof(cccd));
            }    
            _context.Cccds.Add(cccd);
            return _context.SaveChangesAsync();
        }
    }
}
