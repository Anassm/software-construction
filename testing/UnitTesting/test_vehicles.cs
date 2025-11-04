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
    private readonly User _admin;
    private readonly Vehicle _vehicle;

    public VehicleServiceTests()
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
            BirthDate = new DateTime(1990, 1, 1),
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
            BirthDate = new DateTime(1990, 1, 1),
            IsActive = true,
            Vehicles = new List<Vehicle>(),
            Sessions = new List<Session>(),
            Reservations = new List<Reservation>()
        };
        _context.Users.Add(_admin);
        _context.Users.Add(_user);

        _vehicle = new Vehicle
        {
            LicensePlate = "ABC123",
            Make = "Toyota",
            Model = "Corolla",
            Color = "Blue",
            Year = 2020,
            UserID = _user.ID
        };
        _context.Vehicles.Add(_vehicle);


        _context.SaveChanges();
        _service = new VehicleService(_context);
    }

    [Fact]
    public async Task CreateVehicle_InvalidUser()
    {
        var vehicle = new CreateVehicleDto
        {
            LicensePlate = "XYZ789",
            Make = "Toyota",
            Model = "Corolla",
            Color = "Blue",
            Year = 2020,
        };
        var result = await _service.CreateVehicleAsync(vehicle, Guid.NewGuid().ToString());
        var vehicleInDb = await _context.Vehicles.AnyAsync(v => v.LicensePlate == vehicle.LicensePlate);
        Assert.Equal(404, result.statusCode);
        Assert.False(vehicleInDb);
    }

    [Fact]
    public async Task CreateVehicle_Success()
    {
        var vehicle = new CreateVehicleDto
        {
            LicensePlate = "XYZ789",
            Make = "Toyota",
            Model = "Corolla",
            Color = "Blue",
            Year = 2020,
        };
        var result = await _service.CreateVehicleAsync(vehicle, _user.IdentityUserId);
        var vehicleInDb = await _context.Vehicles.AnyAsync(v => v.LicensePlate == vehicle.LicensePlate);
        Assert.Equal(201, result.statusCode);
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
        var result = await _service.CreateVehicleAsync(vehicle, _user.IdentityUserId);
        var vehiclesCount = await _context.Vehicles.CountAsync(v => v.LicensePlate == vehicle.LicensePlate);
        Assert.Equal(409, result.statusCode);
        Assert.Equal(1, vehiclesCount);
    }

    [Fact]
    public async Task UpdateVehicle_InvalidUser()
    {
        var vehicle = new UpdateVehicleDto
        {
            LicensePlate = "CBA321",
            Make = "nissan",
            Model = "GTR",
            Color = "Black",
            Year = 2020,
        };
        var existingVehicle = await _context.Vehicles.FirstOrDefaultAsync();
        var result = await _service.UpdateVehicleAsync(existingVehicle!.ID.ToString(), vehicle, Guid.NewGuid().ToString());
        var vehicleInDb = await _context.Vehicles.AnyAsync(v => v.LicensePlate == vehicle.LicensePlate);
        Assert.Equal(404, result.statusCode);
        Assert.False(vehicleInDb);
    }

    [Fact]
    public async Task UpdateVehicle_VehicleNotFound()
    {
        var vehicle = new UpdateVehicleDto
        {
            LicensePlate = "CBA321",
            Make = "nissan",
            Model = "GTR",
            Color = "Black",
            Year = 2020,
        };
        var existingVehicle = await _context.Vehicles.FirstOrDefaultAsync();
        var result = await _service.UpdateVehicleAsync(Guid.NewGuid().ToString(), vehicle, _user.IdentityUserId);
        var vehicleInDb = await _context.Vehicles.AnyAsync(v => v.LicensePlate == vehicle.LicensePlate);
        Assert.Equal(404, result.statusCode);
        Assert.False(vehicleInDb);
    }

    [Fact]
    public async Task UpdateVehicle_DuplicateVehicle()
    {
        var createVehicle = new Vehicle
        {
            LicensePlate = "XYZ789",
            Make = "Toyota",
            Model = "Corolla",
            Color = "Blue",
            Year = 2020,
            UserID = _user.ID
        };
        _context.Vehicles.Add(createVehicle);
        _context.SaveChanges();
        var updateVehicle = new UpdateVehicleDto
        {
            LicensePlate = "XYZ789",
            Make = "Toyota",
            Model = "Corolla",
            Color = "Blue",
            Year = 2020,
        };
        var existingVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == _vehicle.LicensePlate);
        var result = await _service.UpdateVehicleAsync(existingVehicle!.ID.ToString(), updateVehicle, _user.IdentityUserId);
        var vehicleInDb = await _context.Vehicles.Where(v => v.LicensePlate == _vehicle.LicensePlate).CountAsync();
        Assert.Equal(409, result.statusCode);
        Assert.Equal(1, vehicleInDb);
    }

    [Fact]
    public async Task UpdateVehicle_Success()
    {
        var updateVehicle = new UpdateVehicleDto
        {
            LicensePlate = "XYZ789",
            Make = "Toyota",
            Model = "Corolla",
            Color = "Blue",
            Year = 2020,
        };
        var existingVehicle = await _context.Vehicles.FirstOrDefaultAsync();
        var result = await _service.UpdateVehicleAsync(_vehicle.ID.ToString(), updateVehicle, _user.IdentityUserId);
        var vehicleInDb = await _context.Vehicles.AnyAsync(v => v.LicensePlate == updateVehicle.LicensePlate);
        var vehicleCountInDb = await _context.Vehicles.CountAsync();
        Assert.Equal(200, result.statusCode);
        Assert.True(vehicleInDb);
        Assert.Equal(1, vehicleCountInDb);
    }

    [Fact]
    public async Task DeleteVehicle_UserNotFound()
    {
        var result = await _service.DeleteVehicleAsync(_vehicle.ID.ToString(), Guid.NewGuid().ToString());
        var vehicleCountInDb = await _context.Vehicles.CountAsync();
        Assert.Equal(404, result.statusCode);
        Assert.Equal(1, vehicleCountInDb);
    }

    [Fact]
    public async Task DeleteVehicle_VehicleNotFound()
    {
        var result = await _service.DeleteVehicleAsync(Guid.NewGuid().ToString(), _user.IdentityUserId);
        var vehicleCountInDb = await _context.Vehicles.CountAsync();
        Assert.Equal(404, result.statusCode);
        Assert.Equal(1, vehicleCountInDb);
    }

    [Fact]
    public async Task DeleteVehicle_Success()
    {
        var result = await _service.DeleteVehicleAsync(_vehicle.ID.ToString(), _user.IdentityUserId);
        var vehicleCountInDb = await _context.Vehicles.CountAsync();
        Assert.Equal(200, result.statusCode);
        Assert.Equal(0, vehicleCountInDb);
    }

    [Fact]
    public async Task GetAllVehicles_UserNotFound()
    {
        var result = await _service.GetAllVehiclesAsync(Guid.NewGuid().ToString());
        Assert.Equal(404, result.statusCode);
        Assert.Null(result.data);
    }

    [Fact]
    public async Task GetAllVehicles_Success()
    {
        var result = await _service.GetAllVehiclesAsync(_user.IdentityUserId.ToString());
        Assert.Equal(200, result.statusCode);
        Assert.Equal(_vehicle, result.data.First());
    }

    [Fact]
    public async Task GetAllVehiclesForUser_AdminNotFound()
    {
        var result = await _service.GetAllVehiclesForUserAsync("unknown", Guid.NewGuid().ToString());
        Assert.Equal(404, result.statusCode);
        Assert.Null(result.data);
    }

    [Fact]
    public async Task GetAllVehiclesForUser_NotAnAdmin()
    {
        var result = await _service.GetAllVehiclesForUserAsync("adminuser", _user.IdentityUserId);
        Assert.Equal(401, result.statusCode);
        Assert.Null(result.data);
    }

    [Fact]
    public async Task GetAllVehiclesForUser_UserNotFound()
    {
        var result = await _service.GetAllVehiclesForUserAsync("unknown", _admin.IdentityUserId);
        Assert.Equal(404, result.statusCode);
        Assert.Null(result.data);
    }

    [Fact]
    public async Task GetAllVehiclesForUser_Success()
    {
        var result = await _service.GetAllVehiclesForUserAsync("testuser", _admin.IdentityUserId);
        Assert.Equal(200, result.statusCode);
        Assert.NotEmpty(result.data);
    }

    [Fact]
    public async Task GetReservationsByVehicle_UserNotFound()
    {
        var result = await _service.GetReservationsByVehicleAsync(_vehicle.ID.ToString(), Guid.NewGuid().ToString());
        Assert.Equal(404, result.statusCode);
        Assert.Null(result.data);
    }

    [Fact]
    public async Task GetReservationsByVehicle_VehicleNotFound()
    {
        var result = await _service.GetReservationsByVehicleAsync(Guid.NewGuid().ToString(), _user.IdentityUserId);
        Assert.Equal(404, result.statusCode);
        Assert.Null(result.data);
    }

    [Fact]
    public async Task GetReservationsByVehicle_Success()
    {
        var reservation = new Reservation
        {
            ID = Guid.NewGuid(),
            OldID = "",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 100.0f,
            CompanyName = "TestCompany",
            UserID = _user.ID,
            VehicleID = _vehicle.ID,
            ParkingLotID = Guid.NewGuid(),
            User = _user,
            Vehicle = _vehicle,
            ParkingLot = null
        };
        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        var result = await _service.GetReservationsByVehicleAsync(_vehicle.ID.ToString(), _user.IdentityUserId);
        Assert.Equal(200, result.statusCode);
        Assert.Equal(reservation, result.data.First());
    }

    [Fact]
    public async Task GetVehicleHistory_UserNotFound()
    {
        var result = await _service.GetVehicleHistoryAsync(_vehicle.LicensePlate, Guid.NewGuid().ToString());
        Assert.Equal(404, result.statusCode);
        Assert.Null(result.data);
    }

    [Fact]
    public async Task GetVehicleHistory_VehicleNotFound()
    {
        var result = await _service.GetVehicleHistoryAsync("123456", _user.IdentityUserId);
        Assert.Equal(404, result.statusCode);
        Assert.Null(result.data);
    }

    [Fact]
    public async Task GetVehicleHistory_Success()
    {
        var result = await _service.GetVehicleHistoryAsync(_vehicle.LicensePlate, _user.IdentityUserId);
        Assert.Equal(200, result.statusCode);
        Assert.NotNull(result.data);
    }
}
