using DataAccess.DTOs;
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

        public async Task<ProfileDTO?> GetProfileByUserId(int userId)
        {
            var user = await _context.Users
            .Where(u => u.UserId == userId)
            .Select(u => new ProfileDTO
            {
                UserId = u.UserId,
                Fullname = u.Fullname,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Dob = u.Dob,
                Gender = u.Gender,
                Address = u.Address,
                Status = u.Status,
                CccdId = u.Cccds.Select(c => c.CccdId).FirstOrDefault(),  
                ImgFront = u.Cccds.Select(c => c.ImgFront).FirstOrDefault(),
                ImgBack = u.Cccds.Select(c => c.ImgBack).FirstOrDefault(),
                Code = u.Cccds.Select(c => c.Code).FirstOrDefault()
            })
            .FirstOrDefaultAsync();

            return user;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            if (String.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be blank or contain only spaces", nameof(email));
            }

            return await _context.Users.Include(u => u.Role)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetUserByEmailOrPhone(string search)
        {
            return await _context.Users
        .Where(u => u.Email.Contains(search) || u.PhoneNumber.Contains(search))
        .ToListAsync();
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
                throw new ArgumentException("User cannot be null", nameof(newUser));
            }
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}

