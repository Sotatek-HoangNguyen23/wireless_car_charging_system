using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.UserDTO
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public RoleDto Role { get; set; } 
    }

    public class RoleDto
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
    }
}
