using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using v2.infrastructure.Services;
using v2.Infrastructure.Data;
using v2.Core.DTOs;
using v2.Core.Models;
using Xunit;

public class ParkingLotServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly ParkingLotService _service;

    public ParkingLotServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new ParkingLotService(_context);
    }

    private async Task<ParkingLot> CreateParkingLotAsync(int capacity = 2)
    {
        var lot = new ParkingLot
        {
            ID = Guid.NewGuid(),
            Name = "Test Lot",
            Address = "123 Test St",
            Location = "Test City",
            Capacity = capacity,
            Reserved = 0,
            Tariff = 10,
            DayTariff = 100,
            CreatedAt = DateTime.UtcNow,
            latitude = 1,
            longitude = 1,
            Reservations = new List<Reservation>(),
            Sessions = new List<Session>()
        };

        _context.ParkingLots.Add(lot);
        await _context.SaveChangesAsync();
        return lot;
    }

    [Fact]
    public async Task CreateParkingLotAsync_WithUniqueData_ShouldSucceedAndReturn201()
    {
        
        var dto = new ParkingLotCreateRequest
        {
            Name = "Unit Test Lot",
            Address = "123 Test Street",
            Location = "Testville",
            Capacity = 10,
            Tariff = 1,
            DayTariff = 10,
            Latitude = 1,
            Longitude = 1
        };

       
        var (statusCode, message) = await _service.CreateParkingLotAsync(dto);

      
        Assert.Equal(201, statusCode);

        var savedLot = await _context.ParkingLots.FirstOrDefaultAsync();
        Assert.NotNull(savedLot);
        Assert.Equal("Unit Test Lot", savedLot!.Name);
    }

    [Fact]
    public async Task StartSessionAsync_WhenParkingLotDoesNotExist_ShouldReturn404()
    {
        var result = await _service.StartSessionAsync(Guid.NewGuid(), "ABC123", Guid.Empty);
        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task StartSessionAsync_WhenParkingLotIsFull_ShouldReturn409()
    {
        var lot = await CreateParkingLotAsync(capacity: 1);

        _context.Sessions.Add(new Session
        {
            ID = Guid.NewGuid(),
            ParkingLotID = lot.ID,
            LicensePlate = "FULL1",
            StartTime = DateTime.UtcNow,
            PaymentStatus = PaymentStatus.Unpaid
        });
        await _context.SaveChangesAsync();

        var result = await _service.StartSessionAsync(lot.ID, "FULL2", Guid.Empty);
        Assert.Equal(409, result.statusCode);
    }

    [Fact]
    public async Task StartSessionAsync_WhenDuplicateActiveSessionExists_ShouldReturn409()
    {
        var lot = await CreateParkingLotAsync();

        _context.Sessions.Add(new Session
        {
            ID = Guid.NewGuid(),
            ParkingLotID = lot.ID,
            LicensePlate = "DUP123",
            StartTime = DateTime.UtcNow,
            PaymentStatus = PaymentStatus.Unpaid
        });
        await _context.SaveChangesAsync();

        var result = await _service.StartSessionAsync(lot.ID, "DUP123", Guid.Empty);
        Assert.Equal(409, result.statusCode);
    }

    [Fact]
    public async Task StartSessionAsync_WithValidData_ShouldReturn201()
    {
        var lot = await CreateParkingLotAsync();

        var result = await _service.StartSessionAsync(lot.ID, "OK123", Guid.Empty);

        Assert.Equal(201, result.statusCode);
        Assert.Single(_context.Sessions);
    }

    [Fact]
    public async Task StopSessionAsync_WhenNoActiveSession_ShouldReturn404()
    {
        var lot = await CreateParkingLotAsync();

        var result = await _service.StopSessionAsync(lot.ID, "NOPE", Guid.Empty);
        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task StopSessionAsync_WithActiveSession_ShouldReturn200()
    {
        var lot = await CreateParkingLotAsync();

        var session = new Session
        {
            ID = Guid.NewGuid(),
            ParkingLotID = lot.ID,
            ParkingLot = lot,
            LicensePlate = "STOP123",
            StartTime = DateTime.UtcNow.AddHours(-2),
            PaymentStatus = PaymentStatus.Unpaid
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        var result = await _service.StopSessionAsync(lot.ID, "STOP123", Guid.Empty);

        Assert.Equal(200, result.statusCode);

        var updated = await _context.Sessions.FindAsync(session.ID);
        Assert.NotNull(updated!.EndTime);
    }

    [Fact]
    public async Task GetAllSessionsForLotAsync_WhenLotDoesNotExist_ShouldReturn404()
    {
        var result = await _service.GetAllSessionsForLotAsync(Guid.NewGuid());
        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task GetAllSessionsForLotAsync_WhenSessionsExist_ShouldReturn200()
    {
        var lot = await CreateParkingLotAsync();

        _context.Sessions.Add(new Session
        {
            ID = Guid.NewGuid(),
            ParkingLotID = lot.ID,
            LicensePlate = "A1",
            StartTime = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var result = await _service.GetAllSessionsForLotAsync(lot.ID);
        Assert.Equal(200, result.statusCode);
    }

    [Fact]
    public async Task GetSessionByIdAsync_WhenNotFound_ShouldReturn404()
    {
        var lot = await CreateParkingLotAsync();

        var result = await _service.GetSessionByIdAsync(lot.ID, Guid.NewGuid());
        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task GetSessionByIdAsync_WhenFound_ShouldReturn200()
    {
        var lot = await CreateParkingLotAsync();

        var session = new Session
        {
            ID = Guid.NewGuid(),
            ParkingLotID = lot.ID,
            LicensePlate = "FIND123",
            StartTime = DateTime.UtcNow
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        var result = await _service.GetSessionByIdAsync(lot.ID, session.ID);
        Assert.Equal(200, result.statusCode);
    }

    [Fact]
    public async Task DeleteSessionAsync_WhenNotFound_ShouldReturn404()
    {
        var lot = await CreateParkingLotAsync();

        var result = await _service.DeleteSessionAsync(lot.ID, Guid.NewGuid());
        Assert.Equal(404, result.statusCode);
    }

    [Fact]
    public async Task DeleteSessionAsync_WhenFound_ShouldReturn200()
    {
        var lot = await CreateParkingLotAsync();

        var session = new Session
        {
            ID = Guid.NewGuid(),
            ParkingLotID = lot.ID,
            LicensePlate = "DEL123",
            StartTime = DateTime.UtcNow
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteSessionAsync(lot.ID, session.ID);

        Assert.Equal(200, result.statusCode);
        Assert.Empty(_context.Sessions);
    }

    [Fact]
    public async Task CreateParkingLotAsync_WithDuplicateData_ShouldFailAndReturn409()
    {
       
        var existingLot = new ParkingLot
        {
            ID = Guid.NewGuid(),
            Name = "Existing Lot",
            Address = "456 Main St",
            Location = "Old Town",
            Capacity = 20,
            Reserved = 0,
            Tariff = 2,
            DayTariff = 20,
            CreatedAt = DateTime.UtcNow,
            latitude = 1,
            longitude = 1,
            Reservations = new List<Reservation>(),
            Sessions = new List<Session>()
        };
        _context.ParkingLots.Add(existingLot);
        await _context.SaveChangesAsync();

        var dto = new ParkingLotCreateRequest
        {
            Name = "Existing Lot",
            Address = "456 Main St",
            Location = "New Town",
            Capacity = 30,
            Tariff = 3,
            DayTariff = 30,
            Latitude = 1,
            Longitude = 1
        };

      
        var (statusCode, message) = await _service.CreateParkingLotAsync(dto);

        
        Assert.Equal(409, statusCode);
        Assert.Equal(1, await _context.ParkingLots.CountAsync());
    }

    [Fact]
    public async Task GetParkingLotAsync_WithExistingId_ShouldSucceedAndReturn200()
    {

        var lotId = Guid.NewGuid();
        _context.ParkingLots.Add(new ParkingLot
        {
            ID = lotId,
            Name = "Find Me",
            Address = "789 Side St",
            Location = "Someplace",
            Capacity = 5,
            Reserved = 0,
            Tariff = 1,
            DayTariff = 1,
            CreatedAt = DateTime.UtcNow,
            latitude = 1,
            longitude = 1,
            Reservations = new List<Reservation>(),
            Sessions = new List<Session>()
        });
        await _context.SaveChangesAsync();

       
        var (statusCode, message) = await _service.GetParkingLotAsync(lotId);

       
        Assert.Equal(200, statusCode);

       
        var lotInDb = await _context.ParkingLots.FindAsync(lotId);
        Assert.NotNull(lotInDb);
        Assert.Equal("Find Me", lotInDb!.Name);
    }

    [Fact]
    public async Task GetParkingLotAsync_WithNonExistentId_ShouldFailAndReturn404()
    {
        
        var nonExistentId = Guid.NewGuid();

        
        var (statusCode, message) = await _service.GetParkingLotAsync(nonExistentId);

        
        Assert.Equal(404, statusCode);
    }

    [Fact]
    public async Task GetAllParkingLotsAsync_WhenLotsExist_ShouldReturn200AndAllLots()
    {
       
        _context.ParkingLots.AddRange(
            new ParkingLot
            {
                ID = Guid.NewGuid(),
                Name = "Lot A",
                Address = "Addr A",
                Location = "Loc A",
                Capacity = 1,
                Reserved = 0,
                Tariff = 1,
                DayTariff = 1,
                CreatedAt = DateTime.UtcNow,
                latitude = 1,
                longitude = 1,
                Reservations = new List<Reservation>(),
                Sessions = new List<Session>()
            },
            new ParkingLot
            {
                ID = Guid.NewGuid(),
                Name = "Lot B",
                Address = "Addr B",
                Location = "Loc B",
                Capacity = 1,
                Reserved = 0,
                Tariff = 1,
                DayTariff = 1,
                CreatedAt = DateTime.UtcNow,
                latitude = 1,
                longitude = 1,
                Reservations = new List<Reservation>(),
                Sessions = new List<Session>()
            }
        );
        await _context.SaveChangesAsync();

       
        var (statusCode, message) = await _service.GetAllParkingLotsAsync();

    
        Assert.Equal(200, statusCode);
      
        var count = await _context.ParkingLots.CountAsync();
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task UpdateParkingLotAsync_WithValidData_ShouldSucceedAndReturn200()
    {
       
        var lotId = Guid.NewGuid();
        _context.ParkingLots.Add(new ParkingLot
        {
            ID = lotId,
            Name = "Original Name",
            Address = "Original Address",
            Location = "Original Loc",
            Capacity = 10,
            Reserved = 0,
            Tariff = 1,
            DayTariff = 1,
            CreatedAt = DateTime.UtcNow,
            latitude = 1,
            longitude = 1,
            Reservations = new List<Reservation>(),
            Sessions = new List<Session>()
        });
        await _context.SaveChangesAsync();

        var dto = new ParkingLotCreateRequest
        {
            Name = "Updated Name",
            Address = "Updated Address",
            Location = "Updated Loc",
            Capacity = 20,
            Tariff = 2,
            DayTariff = 20,
            Latitude = 2,
            Longitude = 2
        };

      
        var (statusCode, message) = await _service.UpdateParkingLotAsync(lotId, dto);

       
        Assert.Equal(200, statusCode);

        var updated = await _context.ParkingLots.FindAsync(lotId);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated!.Name);
        Assert.Equal(20, updated.Capacity);
    }

    [Fact]
    public async Task DeleteParkingLotAsync_WithExistingId_ShouldSucceedAndReturn200()
    {
        
        var lotId = Guid.NewGuid();
        _context.ParkingLots.Add(new ParkingLot
        {
            ID = lotId,
            Name = "To Be Deleted",
            Address = "Delete Addr",
            Location = "Delete Loc",
            Capacity = 1,
            Reserved = 0,
            Tariff = 1,
            DayTariff = 1,
            CreatedAt = DateTime.UtcNow,
            latitude = 1,
            longitude = 1,
            Reservations = new List<Reservation>(),
            Sessions = new List<Session>()
        });
        await _context.SaveChangesAsync();

    
        var (statusCode, message) = await _service.DeleteParkingLotAsync(lotId);

     
        Assert.Equal(200, statusCode);
        Assert.Equal(0, await _context.ParkingLots.CountAsync());
    }

    [Fact]
    public async Task DeleteParkingLotAsync_WithNonExistentId_ShouldFailAndReturn404()
    {
        
        var nonExistentId = Guid.NewGuid();

      
        var (statusCode, message) = await _service.DeleteParkingLotAsync(nonExistentId);

        
        Assert.Equal(404, statusCode);
    }
}
