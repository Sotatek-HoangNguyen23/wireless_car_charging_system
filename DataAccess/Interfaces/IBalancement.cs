using DataAccess.DTOs;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IBalancement
    {
        Task<BalanceTransaction> AddBalanceTransactionAsync(BalanceTransaction payment);
        Task<BalanceTransaction> UpdateBalanceTransactionStatusAsync(string orderCode, string status);
        Task<BalanceTransaction> GetBalanceTransactionByOrderIdAsync(string orderCode);

        Task AddBalance(Balance balance);
        Task<Balance> GetBalanceByUserId(int userId);

        Task<Balance> UpdateBalance(Balance balance );

        Task<List<TransactionDTO>> GetTransactionHistory( int userId, DateTime? start, DateTime? end);
    }
}
