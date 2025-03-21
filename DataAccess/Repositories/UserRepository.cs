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
        public async Task<User?> GetUserByPhone(string phone)
        {
            if (String.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException("Phone không thể trống hoặc khoảng trắng", nameof(phone));
            }

            return await _context.Users.Include(u => u.Role)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.PhoneNumber == phone);
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.Users.Include(u => u.Role)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == id);
        }
        public async Task<User?> GetUserByCccd(string cccd)
        {
            if (String.IsNullOrWhiteSpace(cccd))
            {
                throw new ArgumentException("CCCD không thể trống hoặc khoảng trắng", nameof(cccd));
            }
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Cccds)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Cccds.Any(c => c.Code == cccd));
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

        public async Task UpdateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentException("User không được null", nameof(user));
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}

