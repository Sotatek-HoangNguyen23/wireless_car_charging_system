using DataAccess.DTOs.Auth;
using DataAccess.DTOs.CarDTO;
using DataAccess.DTOs.UserDTO;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IUserRepository
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserByPhone(string phone);
        Task<User?> GetUserById(int id);
        Task<User?> GetUserByCccd(string cccd);
        Task SaveUser(User user);

        Task <ProfileDTO?> GetProfileByUserId( int userId); 
        Task UpdateUser(User user);

        Task<List<User>> GetUserByEmailOrPhone(string search);
        PagedResult<UserDto> GetUsers(string? searchQuery, string? status, int? roleId, int pageNumber, int pageSize);
        Task ChangeUserStatusAsync(int userId, string newStatus);

        Task DeleteUserReal(int userId);

        Task AddDocumentRequest(DocumentReview document);
        bool IsMailOrPhoneDuplicate(int userId,string mail, string phone);

        Task<bool> HavingDriverLicenseYet(int userId);
    }
}
