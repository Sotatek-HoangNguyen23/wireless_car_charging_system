using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.UserTest
{
    [TestFixture]
    public class UserRepoTest
    {
        private WccsContext _context;
        private UserRepository _repository;
        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<WccsContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
            _context = new WccsContext(options);
            _repository = new UserRepository(_context);
            var roles = new List<Role>
            {
                new Role { RoleId = 1, RoleName = "Driver" },
                new Role { RoleId = 2, RoleName = "Station owner" },
                new Role { RoleId = 3, RoleName = "Operator" },
                new Role { RoleId = 4, RoleName = "Admin" }
            };
            var user = new User
            {
                UserId = 1,
                RoleId = 1,
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                CreateAt = DateTime.UtcNow
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

    }

}
