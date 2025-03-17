using DataAccess.DTOs.Auth;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly WccsContext _context;
        public UserRepository(WccsContext context)
        {
            _context = context;
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
        public async Task<User?> GetUserByEmail(string email)
        {
            if (String.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email không thể trống hoặc khoảng trắng", nameof(email));
            }

            return await _context.Users.Include(u => u.Role)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.Users.Include(u => u.Role)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == id);
        }


        public async Task SaveUser(User newUser)
        {
            if (newUser == null)
            {
                throw new ArgumentException("User không được null", nameof(newUser));
            }
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
        }
    }
}

