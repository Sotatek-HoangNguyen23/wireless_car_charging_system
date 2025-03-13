using DataAccess.DTOs.Auth;
using DataAccess.Models;
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
        Task<User?> GetUserById(int id);
        Task SaveUser(User user);
    }
}
