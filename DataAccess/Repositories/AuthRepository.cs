using BCrypt.Net;
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
    public class AuthRepository : IAuthRepository
    {
        private readonly WccsContext _context;
        public AuthRepository(WccsContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> FindRefreshToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token không thể trống hoặc khoảng trắng", nameof(token));
            }
           
                return await _context.RefreshTokens
                    .SingleOrDefaultAsync(rt=>rt.Token == token);
        }
        public async Task SaveRefreshToken(string token, User user)
        {

            if(user == null)
            {
                throw new ArgumentNullException(nameof(user), "Người dùng không thể null.");
            }  
            var dbUser = await _context.Users.FindAsync(user.UserId);
            if (dbUser == null)
            {
                throw new InvalidOperationException($"Người dùng với UserId {user.UserId} không tồn tại.");
            }

            // Nếu tồn tại, tạo refresh token và lưu vào DB
            RefreshToken refreshToken = new RefreshToken
            {
                Token = token,
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                // Thay đổi Revoked thành false khi tạo mới
                 Revoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            if (refreshToken == null)
            {
                throw new ArgumentNullException(nameof(refreshToken), "Refresh token không thể null.");
            }
            var dbToken = await _context.RefreshTokens.FindAsync(refreshToken.TokenId);
            if (dbToken == null)
            {
                throw new InvalidOperationException($"Refresh token với TokenId {refreshToken.TokenId} không tồn tại.");
            }
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}
