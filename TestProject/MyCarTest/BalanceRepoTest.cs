using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.MyCarTest
{
    [TestFixture]
    public class BalanceRepoTest
    {
        //private WccsContext _context;
        //private BalanceRepo _repository;


        //[SetUp]
        //public async Task Setup()
        //{
        //    var options = new DbContextOptionsBuilder<WccsContext>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        //        .Options;

        //    _context = new WccsContext(options);

        //    // Use constructor injection to supply the context
        //    _repository = new BalanceRepo(_context);

        //    // Set up base test data
        //    await SetupTestData();
        //}

        //[TearDown]
        //public void TearDown()
        //{
        //    _context?.Dispose();
        //}

        //private async Task SetupTestData()
        //{
        //    // Create users
        //    var users = new List<User>
        //    {
        //        new User { UserId = 1, Email = "user1@test.com", Fullname = "Test User 1", PhoneNumber = "0123456789" },
        //        new User { UserId = 2, Email = "user2@test.com", Fullname = "Test User 2", PhoneNumber = "0987654321" }
        //    };

        //    // Create balances
        //    var balances = new List<Balance>
        //    {
        //        new Balance {
        //            BalanceId = 1,
        //            UserId = 1,
        //            Balance1 = 1000000,
        //            CreateAt = DateTime.Now.AddDays(-30),
        //            UpdateAt = DateTime.Now.AddDays(-5)
        //        },
        //        new Balance {
        //            BalanceId = 2,
        //            UserId = 2,
        //            Balance1 = 0, // Zero balance for boundary testing
        //            CreateAt = DateTime.Now.AddDays(-15),
        //            UpdateAt = DateTime.Now.AddDays(-15)
        //        }
        //    };

        //    // Create balance transactions
        //    var transactions = new List<BalanceTransaction>
        //    {
        //        new BalanceTransaction {
        //            TransactionId = 1,
        //            BalanceId = 1,
        //            Amount = 500000,
        //            OrderCode = "ORDER123",
        //            Status = "Success",
        //            TransactionType = "Deposit",
        //            TransactionDate = DateTime.Now.AddDays(-20)
        //        },
        //        new BalanceTransaction {
        //            TransactionId = 2,
        //            BalanceId = 1,
        //            Amount = -200000,
        //            OrderCode = "ORDER456",
        //            Status = "Success",
        //            TransactionType = "Withdrawal",
        //            TransactionDate = DateTime.Now.AddDays(-10)
        //        },
        //        new BalanceTransaction {
        //            TransactionId = 3,
        //            BalanceId = 1,
        //            Amount = 700000,
        //            OrderCode = "ORDER789",
        //            Status = "Pending",
        //            TransactionType = "Deposit",
        //            TransactionDate = DateTime.Now.AddDays(-5)
        //        }
        //    };

        //    await _context.Users.AddRangeAsync(users);
        //    await _context.Balances.AddRangeAsync(balances);
        //    await _context.BalanceTransactions.AddRangeAsync(transactions);
        //    await _context.SaveChangesAsync();
        //}


        //// Tests for AddBalanceTransactionAsync
        //[Test]
        //public async Task AddBalanceTransactionAsync_ValidTransaction_AddsToDatabase()
        //{
        //    // Arrange
        //    var initialCount = _context.BalanceTransactions.Count();
        //    var transaction = new BalanceTransaction
        //    {
        //        BalanceId = 1,
        //        Amount = 100000,
        //        OrderCode = "NEWTRANS01",
        //        Status = "Pending",
        //        TransactionType = "Deposit",
        //        TransactionDate = DateTime.Now
        //    };

        //    // Act
        //    var result = await _repository.AddBalanceTransactionAsync(transaction);

        //    // Assert
        //    Assert.That(_context.BalanceTransactions.Count(), Is.EqualTo(initialCount + 1));
        //    Assert.That(result.OrderCode, Is.EqualTo("NEWTRANS01"));
        //    Assert.That(result.TransactionId, Is.GreaterThan(0));
        //}

        //[Test]
        //public async Task AddBalanceTransactionAsync_ZeroAmount_StillAddsToDatabase()
        //{
        //    // Arrange - Boundary value: zero amount transaction
        //    var transaction = new BalanceTransaction
        //    {
        //        BalanceId = 1,
        //        Amount = 0, // Zero amount as boundary value
        //        OrderCode = "ZEROTRANS",
        //        Status = "Pending",
        //        TransactionType = "Deposit",
        //        TransactionDate = DateTime.Now
        //    };

        //    // Act
        //    var result = await _repository.AddBalanceTransactionAsync(transaction);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.Amount, Is.EqualTo(0));

        //    var savedTransaction = await _context.BalanceTransactions
        //        .FirstOrDefaultAsync(t => t.OrderCode == "ZEROTRANS");
        //    Assert.That(savedTransaction, Is.Not.Null);
        //}

        //[Test]
        //public async Task AddBalanceTransactionAsync_MinValueAmount_StillAddsToDatabase()
        //{
        //    // Arrange - Boundary value: minimum possible value for double
        //    var transaction = new BalanceTransaction
        //    {
        //        BalanceId = 1,
        //        Amount = 1, // Extreme boundary value
        //        OrderCode = "MINTRANS",
        //        Status = "Pending",
        //        TransactionType = "Withdrawal",
        //        TransactionDate = DateTime.Now
        //    };

        //    // Act
        //    var result = await _repository.AddBalanceTransactionAsync(transaction);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.Amount, Is.EqualTo(double.MinValue));

        //    var savedTransaction = await _context.BalanceTransactions
        //        .FirstOrDefaultAsync(t => t.OrderCode == "MINTRANS");
        //    Assert.That(savedTransaction, Is.Not.Null);
        //    Assert.That(savedTransaction.Amount, Is.EqualTo(double.MinValue));
        //}

        //[Test]
        //public async Task AddBalanceTransactionAsync_NegativeValueAmount_NotAddsToDatabase()
        //{
           
        //    var transaction = new BalanceTransaction
        //    {
        //        BalanceId = 1,
        //        Amount = -1000000, 
        //        OrderCode = "NEGATIVE",
        //        Status = "Pending",
        //        TransactionType = "Deposit",
        //        TransactionDate = DateTime.Now
        //    };

        //    // Act
        //    var result = await _repository.AddBalanceTransactionAsync(transaction);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
           

        //    var savedTransaction = await _context.BalanceTransactions
        //        .FirstOrDefaultAsync(t => t.OrderCode == "NEGATIVE");
        //    Assert.That(savedTransaction, Is.Not.Null);
            
        //}

        //// Tests for GetBalanceByUserId
        //[Test]
        //public async Task GetBalanceByUserId_ExistingUser_ReturnsBalance()
        //{
        //    // Act
        //    var result = await _repository.GetBalanceByUserId(1);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.UserId, Is.EqualTo(1));
        //    Assert.That(result.Balance1, Is.EqualTo(1000000));
        //}

        //[Test]
        //public async Task GetBalanceByUserId_NonExistingUser_ReturnsNull()
        //{
        //    // Act
        //    var result = await _repository.GetBalanceByUserId(999);

        //    // Assert
        //    Assert.That(result, Is.Null);
        //}

        //// Tests for GetBalanceTransactionByOrderIdAsync
        //[Test]
        //public async Task GetBalanceTransactionByOrderIdAsync_ExistingOrderCode_ReturnsTransaction()
        //{
        //    // Act
        //    var result = await _repository.GetBalanceTransactionByOrderIdAsync("ORDER123");

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.OrderCode, Is.EqualTo("ORDER123"));
        //    Assert.That(result.Amount, Is.EqualTo(500000));
        //}

        //[Test]
        //public async Task GetBalanceTransactionByOrderIdAsync_NonExistingOrderCode_ReturnsNull()
        //{
        //    // Act
        //    var result = await _repository.GetBalanceTransactionByOrderIdAsync("NONEXISTENT");

        //    // Assert
        //    Assert.That(result, Is.Null);
        //}

        //// Tests for GetTransactionHistory
        //[Test]
        //public async Task GetTransactionHistory_ExistingUserWithTransactions_ReturnsAllTransactions()
        //{
        //    // Act
        //    var result = await _repository.GetTransactionHistory(1, null, null);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.Count, Is.EqualTo(3));
        //    Assert.That(result.All(t => t.UserId == 1), Is.True);
        //}

        //[Test]
        //public async Task GetTransactionHistory_NonExistingUser_ReturnsEmptyList()
        //{
        //    // Act
        //    var result = await _repository.GetTransactionHistory(999, null, null);

        //    // Assert
        //    Assert.That(result, Is.Empty);
        //}

        //[Test]
        //public async Task GetTransactionHistory_UserWithNoTransactions_ReturnsEmptyList()
        //{
        //    // Act
        //    var result = await _repository.GetTransactionHistory(2, null, null);

        //    // Assert
        //    Assert.That(result, Is.Empty);
        //}


        //[Test]
        //public async Task GetTransactionHistory_WithStartAndEndDateFilter_ReturnsFilteredTransactions()
        //{
        //    // Act - Filter for transactions within a date range
        //    var startDate = DateTime.Now.AddDays(-15);
        //    var endDate = DateTime.Now.AddDays(-7);
        //    var result = await _repository.GetTransactionHistory(1, startDate, endDate);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.Count, Is.EqualTo(1)); // Should return only transactions within the date range
        //    Assert.That(result.All(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate), Is.True);
        //}

        //[Test]
        //public async Task GetTransactionHistory_WithDateBoundaryValues_ReturnsCorrectData()
        //{
        //    // Arrange - Add a transaction exactly at the boundary date
        //    var boundaryDate = DateTime.Now.AddDays(-15);
        //    var boundaryTransaction = new BalanceTransaction
        //    {
        //        TransactionId = 4,
        //        BalanceId = 1,
        //        Amount = 50000,
        //        OrderCode = "BOUNDARYTX",
        //        Status = "Success",
        //        TransactionType = "Deposit",
        //        TransactionDate = boundaryDate
        //    };
        //    await _context.BalanceTransactions.AddAsync(boundaryTransaction);
        //    await _context.SaveChangesAsync();

        //    // Act
        //    var startResult = await _repository.GetTransactionHistory(1, boundaryDate, null);
        //    var endResult = await _repository.GetTransactionHistory(1, null, boundaryDate);
        //    var exactResult = await _repository.GetTransactionHistory(1, boundaryDate, boundaryDate);

        //    // Assert
        //    Assert.That(startResult.Any(t => t.OrderCode == "BOUNDARYTX"), Is.True);
        //    Assert.That(endResult.Any(t => t.OrderCode == "BOUNDARYTX"), Is.True);
        //    Assert.That(exactResult.Any(t => t.OrderCode == "BOUNDARYTX"), Is.True);
        //    Assert.That(exactResult.Count, Is.EqualTo(1));
        //}

        //// Tests for UpdateBalance
        //[Test]
        //public async Task UpdateBalance_ExistingBalance_UpdatesSuccessfully()
        //{
        //    // Arrange
        //    var balance = await _context.Balances.FindAsync(1);
        //    balance.Balance1 = 1500000; // Update the balance amount

        //    // Act
        //    var result = await _repository.UpdateBalance(balance);

        //    // Assert
        //    Assert.That(result.Balance1, Is.EqualTo(1500000));

        //    // Verify the update was persisted to the database
        //    var updatedBalance = await _context.Balances.FindAsync(1);
        //    Assert.That(updatedBalance.Balance1, Is.EqualTo(1500000));
            
        //}

        //[Test]
        //public void UpdateBalance_NonExistingBalance_ThrowsException()
        //{
        //    // Arrange
        //    var nonExistingBalance = new Balance
        //    {
        //        BalanceId = 999,
        //        UserId = 999,
        //        Balance1 = 5000000
        //    };

        //    // Act & Assert
        //    var ex = Assert.ThrowsAsync<Exception>(async () =>
        //        await _repository.UpdateBalance(nonExistingBalance));
        //    Assert.That(ex.Message, Does.Contain("Balance not found"));
        //}

        //// Tests for UpdateBalanceTransactionStatusAsync
        //[Test]
        //public async Task UpdateBalanceTransactionStatusAsync_ExistingOrderCode_UpdatesStatus()
        //{
        //    // Arrange
        //    string orderCode = "ORDER789";
        //    string newStatus = "PAID";

        //    // Act
        //    var result = await _repository.UpdateBalanceTransactionStatusAsync(orderCode, newStatus);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.Status, Is.EqualTo(newStatus));

        //    // Verify the update was persisted to the database
        //    var updatedTransaction = await _context.BalanceTransactions
        //        .FirstOrDefaultAsync(t => t.OrderCode == orderCode);
        //    Assert.That(updatedTransaction.Status, Is.EqualTo(newStatus));
        //}

        //[Test]
        //public async Task UpdateBalanceTransactionStatusAsync_NonExistingOrderCode_ReturnsNull()
        //{
        //    // Act
        //    var result = await _repository.UpdateBalanceTransactionStatusAsync("NONEXISTENT", "PAID");

        //    // Assert
        //    Assert.That(result, Is.Null);
        //}
    }
}
