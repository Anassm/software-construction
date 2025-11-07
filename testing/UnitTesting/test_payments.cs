using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Xunit;
using v2.Infrastructure.Data;
using v2.Core.Models;
using v2.Core.DTOs;
using System.Linq;
using v2.Infrastructure.Services;

public class PaymentServiceTests
{
    private readonly PaymentService _service;
    private readonly ApplicationDbContext _context;
    private readonly User _user;
    private readonly User _admin;
    private readonly Payment _payment;

    public PaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _user = CreateTestUser("testuser", "user");
        _admin = CreateTestUser("adminuser", "Admin");

        _context.Users.Add(_admin);
        _context.Users.Add(_user);

        _payment = new Payment
        {
            ID = Guid.Parse("f3c5709d-5122-43cb-bccf-0157b7528d02"),
            Amount = 10.50M,
            Initiator = _user.Username,
            CreatedAt = new DateTime(2025, 11, 05, 0, 0, 14, DateTimeKind.Utc),
            CompletedAt = null,
            Hash = null,
            TransactionAmount = 10.50M,
            TransactionDate = new DateTime(2025, 11, 04, 12, 0, 0, DateTimeKind.Utc),
            TransactionMethod = "iDEAL",
            TransactionIssuer = "ABN",
            TransactionBank = "ING",
            SessionID = new Guid("33333333-AAAA-BBBB-CCCC-111111111111"),
            OldID = string.Empty
        };
        _context.Payments.Add(_payment);

        _context.SaveChanges();
        _service = new PaymentService(_context);
    }


    private User CreateTestUser(string username, string role)
    {
        var identityUser = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = username
        };

        return new User
        {
            ID = Guid.NewGuid(),
            IdentityUserId = identityUser.Id,
            IdentityUser = identityUser,
            Username = username,
            Name = username + " Name",
            Email = $"{username}@example.com",
            PhoneNumber = "1234567890",
            Role = role,
            BirthYear = 1990,
            IsActive = true,

            Vehicles = new List<Vehicle>(),
            Sessions = new List<Session>(),
            Reservations = new List<Reservation>()
        };
    }


    [Fact]
    public async Task CreatePayment_Success_WithSessionID()
    {
        var request = new CreatePaymentRequestDTO
        {
            Amount = 25.00M,
            SessionID = Guid.NewGuid(),
            TransactionAmount = 25.50M
        };

        var result = await _service.CreatePaymentAsync(request, _user.IdentityUserId);

        Assert.Equal(201, result.statusCode);
        Assert.NotNull(result.data);
        var paymentsInDb = await _context.Payments.CountAsync();
        Assert.Equal(2, paymentsInDb);
    }

    [Fact]
    public async Task CreatePayment_Success_WithTransaction()
    {
        var request = new CreatePaymentRequestDTO
        {
            Amount = 30.00M,
            Transaction = "TXN123456",
        };

        var result = await _service.CreatePaymentAsync(request, _user.IdentityUserId);

        Assert.Equal(201, result.statusCode);
        Assert.NotNull(result.data);
        var paymentInDb = await _context.Payments.OrderBy(p => p.CreatedAt).LastAsync();
        Assert.Equal("TXN123456", paymentInDb.OldID);
    }

    [Fact]
    public async Task CreatePayment_UserNotFound_Returns404()
    {
        var request = new CreatePaymentRequestDTO
        {
            Amount = 15.00M,
            SessionID = Guid.NewGuid()
        };
        var nonExistentIdentityId = Guid.NewGuid().ToString();

        var result = await _service.CreatePaymentAsync(request, nonExistentIdentityId);

        Assert.Equal(404, result.statusCode);
        var paymentsInDb = await _context.Payments.CountAsync();
        Assert.Equal(1, paymentsInDb);
    }

    [Fact]
    public async Task CreatePayment_MissingRequiredFields_Returns400()
    {
        var request = new CreatePaymentRequestDTO
        {
            Amount = 15.00M,
            SessionID = null,
            Transaction = null
        };

        var result = await _service.CreatePaymentAsync(request, _user.IdentityUserId);

        Assert.Equal(400, result.statusCode);
        Assert.Contains("Required field missing", result.data.ToString());
    }


    [Fact]
    public async Task ConfirmPayment_Success()
    {
        var paymentId = _payment.ID;
        var dto = new ConfirmPaymentRequestDTO { Validation = "VALID_HASH_XYZ", T_Data = new { } };

        var result = await _service.ConfirmPaymentAsync(paymentId, dto, _user.IdentityUserId);

        Assert.Equal(200, result.statusCode);
        var confirmedPayment = await _context.Payments.FindAsync(paymentId);
        Assert.NotNull(confirmedPayment!.CompletedAt);
        Assert.Equal("VALID_HASH_XYZ", confirmedPayment.Hash);
    }

    [Fact]
    public async Task ConfirmPayment_PaymentNotFound_Returns404()
    {
        var nonExistentPaymentId = Guid.NewGuid();
        var dto = new ConfirmPaymentRequestDTO { Validation = "VALID_HASH_XYZ", T_Data = new { } };

        var result = await _service.ConfirmPaymentAsync(nonExistentPaymentId, dto, _user.IdentityUserId);

        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task ConfirmPayment_UserNotFound_Returns404()
    {
        var paymentId = _payment.ID;
        var dto = new ConfirmPaymentRequestDTO { Validation = "VALID_HASH_XYZ", T_Data = new { } };
        var nonExistentIdentityId = Guid.NewGuid().ToString();

        var result = await _service.ConfirmPaymentAsync(paymentId, dto, nonExistentIdentityId);

        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task ConfirmPayment_ForbiddenUser_Returns403()
    {
        var otherUser = CreateTestUser("otheruser", "user");
        _context.Users.Add(otherUser);
        await _context.SaveChangesAsync();

        var dto = new ConfirmPaymentRequestDTO { Validation = "VALID_HASH_XYZ", T_Data = new { } };

        var result = await _service.ConfirmPaymentAsync(_payment.ID, dto, otherUser.IdentityUserId);

        Assert.Equal(403, result.statusCode);
    }

    [Fact]
    public async Task ConfirmPayment_AdminConfirmingOtherUserPayment_Success()
    {
        var paymentId = _payment.ID;
        var dto = new ConfirmPaymentRequestDTO { Validation = "ADMIN_HASH_CONFIRM", T_Data = new { } };

        var result = await _service.ConfirmPaymentAsync(paymentId, dto, _admin.IdentityUserId);

        Assert.Equal(200, result.statusCode);
        var confirmedPayment = await _context.Payments.FindAsync(paymentId);
        Assert.NotNull(confirmedPayment!.CompletedAt);
        Assert.Equal("ADMIN_HASH_CONFIRM", confirmedPayment.Hash);
    }

    [Fact]
    public async Task ConfirmPayment_InvalidHash_Returns401()
    {
        var paymentId = _payment.ID;
        var dto = new ConfirmPaymentRequestDTO { Validation = "invalid_hash", T_Data = new { } };

        var result = await _service.ConfirmPaymentAsync(paymentId, dto, _user.IdentityUserId);

        Assert.Equal(401, result.statusCode);
        var payment = await _context.Payments.FindAsync(paymentId);
        Assert.Null(payment!.CompletedAt);
    }


    [Fact]
    public async Task GetPaymentsByUser_Success_ForOwnUser()
    {
        var payment2 = new Payment
        {
            ID = Guid.NewGuid(),
            Amount = 5.00M,
            Initiator = _user.Username,
            CreatedAt = DateTime.UtcNow,
            TransactionAmount = 5.00M,
            TransactionDate = DateTime.UtcNow,
            TransactionMethod = "iDEAL",
            TransactionIssuer = "ABN",
            TransactionBank = "ING",
            OldID = string.Empty
        };
        _context.Payments.Add(payment2);
        await _context.SaveChangesAsync();

        var result = await _service.GetPaymentsByUserAsync(null, _user.IdentityUserId);

        Assert.Equal(200, result.statusCode);
        var payments = Assert.IsType<List<PaymentResponseDTO>>(result.data);
        Assert.Equal(2, payments.Count);
        Assert.True(payments.All(p => p.Initiator == _user.Username));
    }

    [Fact]
    public async Task GetPaymentsByUser_UserNotFound_Returns404()
    {
        var result = await _service.GetPaymentsByUserAsync(null, Guid.NewGuid().ToString());

        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task GetPaymentsByUser_AdminViewOtherUser_Success()
    {

        var result = await _service.GetPaymentsByUserAsync(_user.Username, _admin.IdentityUserId);

        Assert.Equal(200, result.statusCode);
        var payments = Assert.IsType<List<PaymentResponseDTO>>(result.data);
        Assert.Single(payments);
        Assert.True(payments.All(p => p.Initiator == _user.Username));
    }

    [Fact]
    public async Task GetPaymentsByUser_NonAdminViewOtherUser_Returns403()
    {
        var result = await _service.GetPaymentsByUserAsync(_admin.Username, _user.IdentityUserId);

        Assert.Equal(403, result.statusCode);
    }

    [Fact]
    public async Task GetPaymentsByUser_AdminViewNonExistentUser_Returns404()
    {
        var result = await _service.GetPaymentsByUserAsync("nonexistentuser", _admin.IdentityUserId);

        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task GetPaymentsByUser_NoPaymentsFound_Returns200EmptyList()
    {
        var userWithoutPayments = CreateTestUser("nopayuser", "user");
        _context.Users.Add(userWithoutPayments);
        await _context.SaveChangesAsync();

        var result = await _service.GetPaymentsByUserAsync(null, userWithoutPayments.IdentityUserId);

        Assert.Equal(200, result.statusCode);
        var payments = Assert.IsType<List<PaymentResponseDTO>>(result.data);
        Assert.Empty(payments);
    }


    [Fact]
    public async Task RefundPayment_Success()
    {
        _payment.CompletedAt = DateTime.UtcNow.AddHours(-1);
        await _context.SaveChangesAsync();

        var request = new RefundPaymentRequestDTO { PaymentId = _payment.ID, Reason = "Customer request" };

        var result = await _service.RefundPaymentAsync(request, _admin.IdentityUserId);

        Assert.Equal(201, result.statusCode);
        var refundedPayment = await _context.Payments.FindAsync(_payment.ID);
        Assert.NotNull(refundedPayment!.CompletedAt);
        Assert.StartsWith("REFUND:Customer request", refundedPayment.Hash!);
    }

    [Fact]
    public async Task RefundPayment_NotAdmin_Returns403()
    {
        var request = new RefundPaymentRequestDTO { PaymentId = _payment.ID, Reason = "Test" };

        var result = await _service.RefundPaymentAsync(request, _user.IdentityUserId);

        Assert.Equal(403, result.statusCode);
        var payment = await _context.Payments.FindAsync(_payment.ID);
        Assert.Null(payment!.Hash);
    }

    [Fact]
    public async Task RefundPayment_PaymentNotFound_Returns404()
    {
        var nonExistentPaymentId = Guid.NewGuid();
        var request = new RefundPaymentRequestDTO { PaymentId = nonExistentPaymentId, Reason = "Test" };

        var result = await _service.RefundPaymentAsync(request, _admin.IdentityUserId);

        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task RefundPayment_AlreadyRefunded_Returns409()
    {
        _payment.Hash = "REFUND:PreviousRefund:20251101000000";
        _context.Payments.Update(_payment);
        await _context.SaveChangesAsync();

        var request = new RefundPaymentRequestDTO { PaymentId = _payment.ID, Reason = "Retry" };

        var result = await _service.RefundPaymentAsync(request, _admin.IdentityUserId);

        Assert.Equal(409, result.statusCode);
    }


    [Fact]
    public async Task UpdatePayment_Success_ByInitiator()
    {
        var paymentId = _payment.ID;
        var dto = new UpdatePaymentRequestDTO
        {
            Amount = 12.00M,
            TransactionMethod = "CreditCard",
            CompletedAt = DateTime.UtcNow
        };

        var result = await _service.UpdatePaymentAsync(paymentId, dto, _user.IdentityUserId);

        Assert.Equal(200, result.statusCode);
        var updatedPayment = await _context.Payments.FindAsync(paymentId);
        Assert.Equal(12.00M, updatedPayment!.Amount);
        Assert.Equal("CreditCard", updatedPayment.TransactionMethod);
        Assert.NotNull(updatedPayment.CompletedAt);
    }

    [Fact]
    public async Task UpdatePayment_Success_ByAdmin()
    {
        var paymentId = _payment.ID;
        var dto = new UpdatePaymentRequestDTO
        {
            Hash = "UPDATED_ADMIN_HASH",
            TransactionAmount = 99.99M
        };

        var result = await _service.UpdatePaymentAsync(paymentId, dto, _admin.IdentityUserId);

        Assert.Equal(200, result.statusCode);
        var updatedPayment = await _context.Payments.FindAsync(paymentId);
        Assert.Equal("UPDATED_ADMIN_HASH", updatedPayment!.Hash);
        Assert.Equal(99.99M, updatedPayment.TransactionAmount);
    }

    [Fact]
    public async Task UpdatePayment_PaymentNotFound_Returns404()
    {
        var nonExistentPaymentId = Guid.NewGuid();
        var dto = new UpdatePaymentRequestDTO { Amount = 1.00M };

        var result = await _service.UpdatePaymentAsync(nonExistentPaymentId, dto, _user.IdentityUserId);

        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task UpdatePayment_ForbiddenUser_Returns403()
    {
        var otherUser = CreateTestUser("otheruser2", "user");
        _context.Users.Add(otherUser);
        await _context.SaveChangesAsync();

        var dto = new UpdatePaymentRequestDTO { Amount = 1.00M };

        var result = await _service.UpdatePaymentAsync(_payment.ID, dto, otherUser.IdentityUserId);

        Assert.Equal(403, result.statusCode);
        var payment = await _context.Payments.FindAsync(_payment.ID);
        Assert.Equal(10.50M, payment!.Amount);
    }
}