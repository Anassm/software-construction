using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Xunit;
using v2.Infrastructure.Data;
using v2.Core.Models;
using v2.Core.DTOs;
using System.Reflection.Metadata;
using System.Reflection;
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
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unieke naam per test
            .Options;

        _context = new ApplicationDbContext(options);

        var identityUser = new IdentityUser
        {
            UserName = "testuser"
        };

        var identityAdmin = new IdentityUser
        {
            UserName = "adminuser"
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
            SessionID = new Guid("33333333-AAAA-BBBB-CCCC-111111111111")
        };
        _context.Payments.Add(_payment);


        _context.SaveChanges();
        _service = new PaymentService(_context);
    }

}