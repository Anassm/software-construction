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
