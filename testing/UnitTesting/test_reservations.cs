using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;
using v2.Infrastructure.Data;
using v2.Core.Models;
using v2.infrastructure.Services;
using v2.Core.DTOs;

namespace v2.Infrastructure.Services;

public class ReservationServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly ReservationService _service;
    private readonly ParkingLot _lot;
    private readonly User _user;
    private readonly Vehicle _vehicle;

    public ReservationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"ReservationTest-{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);

        _lot = new ParkingLot
        {
            ID = Guid.NewGuid(),
            Name = "Test Lot",
            Location = "Rotterdam",
            Address = "Airportplein 1",
            Capacity = 100,
            Reserved = 0,
            Tariff = 2.5f,
            DayTariff = 20f,
            CreatedAt = DateTime.UtcNow,
            latitude = 51.95f,
            longitude = 4.45f,
            Reservations = new List<Reservation>(),
            Sessions = new List<Session>()
        };
        _context.ParkingLots.Add(_lot);

        var identityUserId = Guid.NewGuid().ToString();
        var identityUser = new IdentityUser
        {
            Id = identityUserId,
            UserName = "testuser",
            NormalizedUserName = "TESTUSER",
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            EmailConfirmed = true
        };
        _context.Set<IdentityUser>().Add(identityUser);

        _user = new User
        {
            ID = Guid.NewGuid(),
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PhoneNumber = "0000000000",
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            BirthYear = 1990,
            IsActive = true,
            Vehicles = new List<Vehicle>(),
            Reservations = new List<Reservation>(),
            Sessions = new List<Session>(),
            IdentityUserId = identityUserId,
            IdentityUser = identityUser
        };
        _context.Users.Add(_user);

        _vehicle = new Vehicle
        {
            ID = Guid.NewGuid(),
            LicensePlate = "JO-227-4",
            Make = "TestMake",
            Model = "TestModel",
            Color = "Black",
            Year = 2022,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UserID = _user.ID,
            User = _user,
            Reservations = new List<Reservation>()
        };
        _context.Vehicles.Add(_vehicle);

        _context.SaveChanges();

        _service = new ReservationService(_context);
    }

    [Fact]
    public async Task CreateReservation_Should_Fail_When_EndDate_Not_After_StartDate()
    {
        var fixedTime = DateTime.UtcNow; 
        var req = new ReservationCreateRequest
        {
            LicensePlate = _vehicle.LicensePlate, 
            ParkingLotId = _lot.ID, 
            StartDate = fixedTime, 
            EndDate = fixedTime,
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateReservationAsync(req));
        Assert.Contains("EndDate must be greater", ex.Message);
    }

    [Fact]
    public async Task CreateReservation_Should_Fail_When_ParkingLot_NotFound()
    {
        var req = new ReservationCreateRequest
        {
            LicensePlate = _vehicle.LicensePlate,
            StartDate = DateTime.UtcNow.AddHours(1),
            EndDate   = DateTime.UtcNow.AddHours(2),
            ParkingLotId = Guid.NewGuid() // bestaat niet
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateReservationAsync(req));

        Assert.Contains("Parking lot not found", ex.Message);
    }

    [Fact]
    public async Task CreateReservation_Should_Fail_When_Vehicle_NotFound()
    {
        var req = new ReservationCreateRequest
        {
            LicensePlate = "PLATE-NOT-EXISTS",
            StartDate = DateTime.UtcNow.AddHours(1),
            EndDate   = DateTime.UtcNow.AddHours(2),
            ParkingLotId = _lot.ID
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateReservationAsync(req));

        Assert.Contains("Vehicle with given license plate not found", ex.Message);
    }

    [Fact]
    public async Task CreateReservation_Should_Create_When_Valid()
    {
        var start = DateTime.UtcNow.AddHours(1);
        var end   = DateTime.UtcNow.AddHours(3);

        var req = new ReservationCreateRequest
        {
            LicensePlate = _vehicle.LicensePlate,
            StartDate = start,
            EndDate   = end,
            ParkingLotId = _lot.ID
        };

        var created = await _service.CreateReservationAsync(req);
        var saved = await _context.Reservations.SingleAsync(r => r.ID == created.ID);

        Assert.Equal("Pending", saved.Status);
        Assert.Equal(_vehicle.ID, saved.VehicleID);
        Assert.Equal(_lot.ID, saved.ParkingLotID);
        Assert.Equal(_vehicle.UserID, saved.UserID);
        Assert.Equal(start, saved.StartDate);
        Assert.Equal(end, saved.EndDate);
    }
}
