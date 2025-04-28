using API.Services;
using DataAccess.DTOs;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TestProject.MyCarTest
{
    //[TestFixture]
    //public class PaymentServiceTest
    //{
    //    private WccsContext _context;
    //    private BalanceRepo _balanceRepo;

    //    [SetUp]
    //    public async Task Setup()
    //    {
    //        var options = new DbContextOptionsBuilder<WccsContext>()
    //            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    //            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
    //            .Options;

    //        _context = new WccsContext(options);
    //        _balanceRepo = new BalanceRepo(_context);

    //        await SetupTestData();
    //    }

    //    [TearDown]
    //    public void TearDown()
    //    {
    //        _context?.Dispose();
    //    }

    //    private async Task SetupTestData()
    //    {
    //        var users = new List<User>
    //        {
    //            new User { UserId = 1, Email = "user1@test.com", Fullname = "Test User 1", PhoneNumber = "0123456789" },
    //            new User { UserId = 2, Email = "user2@test.com", Fullname = "Test User 2", PhoneNumber = "0987654321" }
    //        };

    //        var balances = new List<Balance>
    //        {
    //            new Balance {
    //                BalanceId = 1,
    //                UserId = 1,
    //                Balance1 = 1000000,
    //                CreateAt = DateTime.Now.AddDays(-30),
    //                UpdateAt = DateTime.Now.AddDays(-5)
    //            },
    //            new Balance {
    //                BalanceId = 2,
    //                UserId = 2,
    //                Balance1 = 0,
    //                CreateAt = DateTime.Now.AddDays(-15),
    //                UpdateAt = DateTime.Now.AddDays(-15)
    //            }
    //        };

    //        var transactions = new List<BalanceTransaction>
    //        {
    //            new BalanceTransaction {
    //                TransactionId = 1,
    //                BalanceId = 1,
    //                Amount = 500000,
    //                OrderCode = "1001",
    //                Status = "PENDING",
    //                TransactionType = "DEPOSIT",
    //                TransactionDate = DateTime.Now.AddDays(-20)
    //            },
    //            new BalanceTransaction {
    //                TransactionId = 2,
    //                BalanceId = 1,
    //                Amount = 200000,
    //                OrderCode = "1002",
    //                Status = "PENDING",
    //                TransactionType = "DEPOSIT",
    //                TransactionDate = DateTime.Now.AddDays(-10)
    //            },
    //            new BalanceTransaction {
    //                TransactionId = 3,
    //                BalanceId = 2,
    //                Amount = 700000,
    //                OrderCode = "2001",
    //                Status = "PENDING",
    //                TransactionType = "DEPOSIT",
    //                TransactionDate = DateTime.Now.AddDays(-5)
    //            }
    //        };

    //        await _context.Users.AddRangeAsync(users);
    //        await _context.Balances.AddRangeAsync(balances);
    //        await _context.BalanceTransactions.AddRangeAsync(transactions);
    //        await _context.SaveChangesAsync();
    //    }

    //    [Test]
    //    public async Task HandlePaymentCallback_SuccessfulPayment_UpdatesBalanceAndTransaction()
    //    {
    //        int orderCode = 1001;
    //        string status = "PAID";
    //        int userId = 1;

    //        var initialBalance = await _balanceRepo.GetBalanceByUserId(userId);
    //        double initialBalanceAmount = initialBalance.Balance1 ?? 0;

    //        var initialTransaction = await _balanceRepo.GetBalanceTransactionByOrderIdAsync(orderCode.ToString());
    //        string initialStatus = initialTransaction.Status;

    //        var paymentService = new PaymentService();

    //        typeof(PaymentService)
    //            .GetField("_balanceRepo", BindingFlags.NonPublic | BindingFlags.Instance)
    //            .SetValue(paymentService, _balanceRepo);

    //        await paymentService.HandlePaymentCallback(orderCode, status, userId);

    //        var updatedBalance = await _balanceRepo.GetBalanceByUserId(userId);
    //        Assert.That(updatedBalance.Balance1, Is.EqualTo(initialBalanceAmount + initialTransaction.Amount));

    //        var updatedTransaction = await _balanceRepo.GetBalanceTransactionByOrderIdAsync(orderCode.ToString());
    //        Assert.That(updatedTransaction.Status, Is.EqualTo(status));
    //    }

    //    [Test]
    //    public async Task HandlePaymentCallback_UnsuccessfulPayment_DoesNotUpdateBalance()
    //    {
    //        int orderCode = 1002;
    //        string status = "FAILED";
    //        int userId = 1;

    //        var initialBalance = await _balanceRepo.GetBalanceByUserId(userId);
    //        double initialBalanceAmount = initialBalance.Balance1 ?? 0;

    //        var paymentService = new PaymentService();

    //        typeof(PaymentService)
    //            .GetField("_balanceRepo", BindingFlags.NonPublic | BindingFlags.Instance)
    //            .SetValue(paymentService, _balanceRepo);

    //        await paymentService.HandlePaymentCallback(orderCode, status, userId);

    //        var updatedBalance = await _balanceRepo.GetBalanceByUserId(userId);
    //        Assert.That(updatedBalance.Balance1, Is.EqualTo(initialBalanceAmount));

    //        var updatedTransaction = await _balanceRepo.GetBalanceTransactionByOrderIdAsync(orderCode.ToString());
    //        Assert.That(updatedTransaction.Status, Is.EqualTo(status));
    //    }

    //    [Test]
    //    public async Task HandlePaymentCallback_NonExistentOrderCode_DoesNothing()
    //    {
    //        int orderCode = 9999;
    //        string status = "PAID";
    //        int userId = 1;

    //        var initialBalance = await _balanceRepo.GetBalanceByUserId(userId);
    //        double initialBalanceAmount = initialBalance.Balance1 ?? 0;

    //        var paymentService = new PaymentService();

    //        typeof(PaymentService)
    //            .GetField("_balanceRepo", BindingFlags.NonPublic | BindingFlags.Instance)
    //            .SetValue(paymentService, _balanceRepo);

    //        await paymentService.HandlePaymentCallback(orderCode, status, userId);

    //        var updatedBalance = await _balanceRepo.GetBalanceByUserId(userId);
    //        Assert.That(updatedBalance.Balance1, Is.EqualTo(initialBalanceAmount));
    //    }

    //    [Test]
    //    public async Task GetPaymentStatus_ExistingOrderId_ReturnsTransaction()
    //    {
    //        int orderId = 1001;

    //        var paymentService = new PaymentService();

    //        typeof(PaymentService)
    //            .GetField("_balanceRepo", BindingFlags.NonPublic | BindingFlags.Instance)
    //            .SetValue(paymentService, _balanceRepo);

    //        var result = await paymentService.GetPaymentStatus(orderId);

    //        Assert.That(result, Is.Not.Null);
    //        Assert.That(result.OrderCode, Is.EqualTo(orderId.ToString()));
    //    }

    //    [Test]
    //    public async Task GetPaymentStatus_NonExistingOrderId_ReturnsNull()
    //    {
    //        int orderId = 9999;

    //        var paymentService = new PaymentService();

    //        typeof(PaymentService)
    //            .GetField("_balanceRepo", BindingFlags.NonPublic | BindingFlags.Instance)
    //            .SetValue(paymentService, _balanceRepo);

    //        var result = await paymentService.GetPaymentStatus(orderId);

    //        Assert.That(result, Is.Null);
    //    }

    //    [Test]
    //    public async Task GetBalanceByUserId_ExistingUser_ReturnsBalance()
    //    {
    //        int userId = 1;

    //        var paymentService = new PaymentService();

    //        typeof(PaymentService)
    //            .GetField("_balanceRepo", BindingFlags.NonPublic | BindingFlags.Instance)
    //            .SetValue(paymentService, _balanceRepo);

    //        var result = await paymentService.GetBalanceByUserId(userId);

    //        Assert.That(result, Is.Not.Null);
    //        Assert.That(result.UserId, Is.EqualTo(userId));
    //        Assert.That(result.Balance1, Is.EqualTo(1000000));
    //    }

    //    [Test]
    //    public async Task GetBalanceByUserId_NonExistingUser_ReturnsNull()
    //    {
    //        int userId = 9999;

    //        var paymentService = new PaymentService();

    //        typeof(PaymentService)
    //            .GetField("_balanceRepo", BindingFlags.NonPublic | BindingFlags.Instance)
    //            .SetValue(paymentService, _balanceRepo);

    //        var result = await paymentService.GetBalanceByUserId(userId);

    //        Assert.That(result, Is.Null);
    //    }

    //    [Test]
    //    public async Task GetTransactionHistory_ExistingUser_ReturnsTransactions()
    //    {
    //        int userId = 1;

    //        var paymentService = new PaymentService();

    //        typeof(PaymentService)
    //            .GetField("_balanceRepo", BindingFlags.NonPublic | BindingFlags.Instance)
    //            .SetValue(paymentService, _balanceRepo);

    //        var result = await paymentService.GetTransactionHistory(userId, null, null);

    //        Assert.That(result, Is.Not.Null);
    //        Assert.That(result.Count, Is.EqualTo(2));
    //        Assert.That(result.All(t => t.UserId == userId), Is.True);
    //    }

    //    [Test]
    //    public async Task GetTransactionHistory_WithDateFilter_ReturnsFilteredTransactions()
    //    {
    //        int userId = 1;
    //        var startDate = DateTime.Now.AddDays(-15);
    //        var endDate = DateTime.Now;

    //        var paymentService = new PaymentService();

    //        typeof(PaymentService)
    //            .GetField("_balanceRepo", BindingFlags.NonPublic | BindingFlags.Instance)
    //            .SetValue(paymentService, _balanceRepo);

    //        var result = await paymentService.GetTransactionHistory(userId, startDate, endDate);

    //        Assert.That(result, Is.Not.Null);
    //        Assert.That(result.Count, Is.EqualTo(1));
    //        Assert.That(result.All(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate), Is.True);
    //    }

    //    [Test]
    //    public async Task GetTransactionHistory_NonExistingUser_ReturnsEmptyList()
    //    {
    //        int userId = 9999;

    //        var paymentService = new PaymentService();

    //        typeof(PaymentService)
    //            .GetField("_balanceRepo", BindingFlags.NonPublic | BindingFlags.Instance)
    //            .SetValue(paymentService, _balanceRepo);

    //        var result = await paymentService.GetTransactionHistory(userId, null, null);

    //        Assert.That(result, Is.Empty);
    //    }
    //}
}