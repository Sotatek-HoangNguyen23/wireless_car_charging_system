using DataAccess.DTOs;
using DataAccess.DTOs.Auth;
using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
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
                throw new ArgumentException("Email không thể trống hoặc khoảng trắng", nameof(email));
            }

            return await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        } 
        public async Task<User?> GetUserByPhone(string phone)
        {
            if (String.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException("Số điện thoại không thể trống hoặc khoảng trắng", nameof(phone));
            }

            return await _context.Users.Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PhoneNumber == phone);
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.Users.Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id);
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
                .FirstOrDefaultAsync(u => u.Cccds.Any(c => c.Code == cccd));
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
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (existingUser != null)
            {
                _context.Entry(existingUser).CurrentValues.SetValues(user);
            }
            else
            {
                // Nếu không tồn tại trong DB, thêm mới
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> GetUserByEmailOrPhone(string search)
        {
            if (search==null)
            {
                throw new ArgumentNullException("Tìm kiếm không thể trống hoặc khoảng trắng", nameof(search));
            }
            return await _context.Users
        .Where(u => u.Email.Contains(search) || u.PhoneNumber.Contains(search))
        .ToListAsync();
        }

        public PagedResult<UserDto> GetUsers(string? searchQuery, string? status, int? roleId, int pageNumber, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(u => u.Fullname.Contains(searchQuery) || u.Email.Contains(searchQuery) || u.PhoneNumber.Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(u => u.Status.Contains(status));
            }

            int totalRecords = query.Count();

            var users = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Fullname = u.Fullname,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Status = u.Status,
                    Role = new RoleDto
                    {
                        RoleId = u.Role.RoleId,
                        Name = u.Role.RoleName
                    }
                }).ToList();

            return new PagedResult<UserDto>(users, totalRecords, pageSize);
        }

        public async Task ChangeUserStatusAsync(int userId, string newStatus)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Status = newStatus;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public PagedResult<FeedbackDto> GetFeedbacks(string?search, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            var query = _context.Feedbacks
                .Where(f =>
                    (string.IsNullOrEmpty(search) || f.User.Email.Contains(search) || f.Message.Contains(search)) &&
                    (!startDate.HasValue || f.CreatedAt >= startDate) &&
                    (!endDate.HasValue || f.CreatedAt <= endDate)
                )
                .Select(f => new FeedbackDto
                {
                    Id = f.FeedbackId,
                    User = f.User.Email,
                    Message = f.Message,
                    Date = f.CreatedAt
                });

            int totalCount = query.Count(); ;
            var data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<FeedbackDto> (data, totalCount, pageSize);
        }

        public async Task<List<Feedback>> GetFeedbackByUserId(int userId)
        {
            return await _context.Feedbacks
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}

