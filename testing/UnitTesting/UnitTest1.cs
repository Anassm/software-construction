using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Xunit;
using v2.Infrastructure.Data;
using v2.Core.Models;
using v2.infrastructure.Services;
using v2.Core.DTOs;
using System.Reflection.Metadata;
using System.Reflection;

public class VehicleServiceTests
{
    private readonly VehicleService _service;
    private readonly ApplicationDbContext _context;
    private readonly User _user;

    public VehicleServiceTests()
    {
        // Set up in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new ApplicationDbContext(options);

        // Create a dummy IdentityUser
        var identityUser = new IdentityUser
        {
            UserName = "testuser"
        };

        // Seed test user
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
            BirthDate = new DateTime(1990, 1, 1),
            IsActive = true,
            Vehicles = new List<Vehicle>(),
            Sessions = new List<Session>(),
            Reservations = new List<Reservation>()
        };

        _context.Users.Add(_user);
        _context.SaveChanges();
        _service = new VehicleService(_context);
    }

    [Fact]
    public async Task CreateVehicle_InvalidUser()
    {
        var vehicle = new CreateVehicleDto
        {
            LicensePlate = "ABC123",
            Make = "Toyota",
            Model = "Corolla",
            Color = "Blue",
            Year = 2020,
        };
        (int statusCode, object message) = await _service.CreateVehicleAsync(vehicle, Guid.NewGuid().ToString());
        var vehicleInDb = await _context.Vehicles.AnyAsync(v => v.LicensePlate == vehicle.LicensePlate);
        Assert.Equal(404, statusCode);
        Assert.False(vehicleInDb);
    }

    [Fact]
    public async Task CreateVehicle_Success()
    {
        var vehicle = new CreateVehicleDto
        {
            LicensePlate = "ABC123",
            Make = "Toyota",
            Model = "Corolla",
            Color = "Blue",
            Year = 2020,
        };
        (int statusCode, object message) = await _service.CreateVehicleAsync(vehicle, _user.IdentityUserId);
        var vehicleInDb = await _context.Vehicles.AnyAsync(v => v.LicensePlate == vehicle.LicensePlate);
        Assert.Equal(201, statusCode);
        Assert.True(vehicleInDb);
    }

    [Fact]
    public async Task CreateVehicle_DuplicateVehicle()
    {
        var vehicle = new CreateVehicleDto
        {
            LicensePlate = "ABC123",
            Make = "Toyota",
            Model = "Corolla",
            Color = "Blue",
            Year = 2020,
        };
        (int statusCode, object message) = await _service.CreateVehicleAsync(vehicle, _user.IdentityUserId);
        var vehiclesCount = await _context.Vehicles.CountAsync(v => v.LicensePlate == vehicle.LicensePlate);
        Assert.Equal(201, statusCode);
        Assert.Equal(1, vehiclesCount);
    }


}
