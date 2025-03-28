using DataAccess.DTOs;
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
    public class BalanceRepo : IBalancement
    {
        private WccsContext _context;
        public BalanceRepo()
        {
            _context = new WccsContext();
        }

        public async Task<BalanceTransaction> AddBalanceTransactionAsync(BalanceTransaction payment)
        {
            _context.BalanceTransactions.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Balance> GetBalanceByUserId(int userId)
        {
            
            return await _context.Balances
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<BalanceTransaction> GetBalanceTransactionByOrderIdAsync(string orderCode)
        {
            return await _context.BalanceTransactions
               .FirstOrDefaultAsync(p => p.OrderCode == orderCode);
        }

        public async Task<List<TransactionDTO>> GetTransactionHistory(int userId, DateTime? start, DateTime? end)
        {
            var query = _context.BalanceTransactions
            .Where(t => t.Balance.UserId == userId )
            .AsQueryable();

            if (start.HasValue)
                query = query.Where(t => t.TransactionDate >= start.Value);

            if (end.HasValue)
                query = query.Where(t => t.TransactionDate <= end.Value);

            return await query.Select(t => new TransactionDTO
            {
                TransactionId = t.TransactionId,
                BalanceId = t.BalanceId,
                UserId = t.Balance.UserId,
                Amount = t.Amount,
                Status = t.Status,
                OrderCode = t.OrderCode,
                TransactionType = t.TransactionType,
                TransactionDate = t.TransactionDate
            }).ToListAsync();
        }

        public async Task<Balance> UpdateBalance(Balance balance)
        {
            var existingBalance = await _context.Balances
            .FirstOrDefaultAsync(b => b.BalanceId == balance.BalanceId);

            if (existingBalance == null)
            {
                throw new Exception("Balance not found");
            }

            // Cập nhật thông tin
            existingBalance.Balance1 = balance.Balance1;
            existingBalance.UpdateAt = DateTime.UtcNow; // Cập nhật thời gian

            // Lưu thay đổi vào database
            _context.Balances.Update(existingBalance);
            await _context.SaveChangesAsync();

            return existingBalance;
        }

        public async Task<BalanceTransaction> UpdateBalanceTransactionStatusAsync(string orderCode, string status)
        {
            var payment = await _context.BalanceTransactions
                .FirstOrDefaultAsync(p => p.OrderCode == orderCode);

            if (payment != null)
            {
                payment.Status = status;
                payment.TransactionDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return payment;
        }
    }
}
