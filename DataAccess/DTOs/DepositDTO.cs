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

    public class TransactionDTO
    {
        public int BalanceId { get; set; }

        public int UserId { get; set; }

        public int TransactionId { get; set; }

        public double? Amount { get; set; }

        public string? Status { get; set; }

        public string? OrderCode { get; set; }

        public string? TransactionType { get; set; }

        public DateTime? TransactionDate { get; set; }
    }
}
