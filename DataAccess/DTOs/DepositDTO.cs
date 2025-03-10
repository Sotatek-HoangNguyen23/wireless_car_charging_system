using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs
{
    public class DepositDTO
    {
        public int OrderId { get; set; }
        public int TotalPrice { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
