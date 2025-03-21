using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.UserDTO
{
    public class EmailRequest()
    {
        public required string Email { get; set; }
    }
}
