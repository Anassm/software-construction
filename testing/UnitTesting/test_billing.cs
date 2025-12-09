using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Xunit;
using v2.Infrastructure.Data;
using v2.Core.Models;
using v2.infrastructure.Services;
using v2.Core.DTOs;
using System.Reflection;

public class BillingServiceTests
{
    private readonly BillingService _service;
    private readonly ApplicationDbContext _context;
    private readonly User _user;
    private readonly User _admin;
    private readonly Invoice _invoice1;
    private readonly Invoice _invoice2;

    public BillingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var identityUser = new IdentityUser
        {
            UserName = "testuser",
            Id = Guid.NewGuid().ToString()
        };

        var identityAdmin = new IdentityUser
        {
            UserName = "adminuser",
            Id = Guid.NewGuid().ToString()
        };

        _user = new User
        {
            ID = Guid.NewGuid(),
            IdentityUserId = identityUser.Id,
            IdentityUser = identityUser,
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Role = "user",
            BirthYear = 1990,
            IsActive = true,
            Vehicles = new List<Vehicle>(),
            Sessions = new List<Session>(),
            Reservations = new List<Reservation>()
        };

        _admin = new User
        {
            ID = Guid.NewGuid(),
            IdentityUserId = identityAdmin.Id,
            IdentityUser = identityAdmin,
            Username = "adminuser",
            Name = "Admin User",
            Email = "admin@example.com",
            PhoneNumber = "1234567890",
            Role = "admin",
            BirthYear = 1990,
            IsActive = true,
            Vehicles = new List<Vehicle>(),
            Sessions = new List<Session>(),
            Reservations = new List<Reservation>()
        };

        _context.Users.Add(_admin);
        _context.Users.Add(_user);
        _context.SaveChanges();

        _invoice1 = new Invoice
        {
            ID = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            TotalAmount = 50.00f,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(4),
            Status = InvoiceStatus.Paid,
            UserID = _user.ID,
            User = _user
        };

        _invoice2 = new Invoice
        {
            ID = Guid.NewGuid(),
            InvoiceNumber = "INV-002",
            TotalAmount = 75.00f,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            DueDate = DateTime.UtcNow.AddDays(9),
            Status = InvoiceStatus.Open,
            UserID = _user.ID,
            User = _user
        };

        _context.Invoices.Add(_invoice1);
        _context.Invoices.Add(_invoice2);
        _context.SaveChanges();

        _service = new BillingService(_context);
    }

    [Fact]
    public async Task GetMyInvoiceHistory_UserNotFound()
    {
        var result = await _service.GetMyInvoiceHistoryAsync(Guid.NewGuid().ToString());
        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task GetMyInvoiceHistory_Success()
    {
        var result = await _service.GetMyInvoiceHistoryAsync(_user.IdentityUserId);
        Assert.Equal(200, result.statusCode);

        
        var dataType = result.data.GetType();
        var invoicesProperty = dataType.GetProperty("invoices");
        var invoices = invoicesProperty.GetValue(result.data) as List<InvoiceSummaryDto>;

        Assert.NotNull(invoices);
        Assert.Equal(2, invoices.Count);
        Assert.Contains(invoices, i => i.InvoiceNumber == "INV-001");
        Assert.Contains(invoices, i => i.InvoiceNumber == "INV-002");
    }



    [Fact]
    public async Task GetMyInvoiceHistory_EmptyList()
    {
        var identityUser = new IdentityUser 
        { 
            UserName = "noinvoices",
            Id = Guid.NewGuid().ToString()
        };
        
        var newUser = new User
        {
            ID = Guid.NewGuid(),
            IdentityUserId = identityUser.Id,
            IdentityUser = identityUser,
            Username = "noinvoices",
            Name = "No Invoices User",
            Email = "noinvoices@example.com",
            PhoneNumber = "1234567890",
            Role = "user",
            BirthYear = 1990,
            IsActive = true,
            Vehicles = new List<Vehicle>(),
            Sessions = new List<Session>(),
            Reservations = new List<Reservation>()
        };
        _context.Users.Add(newUser);
        _context.SaveChanges();

        var result = await _service.GetMyInvoiceHistoryAsync(newUser.IdentityUserId);
        Assert.Equal(200, result.statusCode);

        var dataType = result.data.GetType();
        var invoicesProperty = dataType.GetProperty("invoices");
        var invoices = invoicesProperty.GetValue(result.data) as List<InvoiceSummaryDto>;

        Assert.NotNull(invoices);
        Assert.Empty(invoices);
    }

}