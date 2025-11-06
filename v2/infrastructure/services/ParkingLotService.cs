// File: ParkingLotService.cs
// Mirrors VehicleService.cs style: tuple returns, EF Core, DI

namespace v2.infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using ChefServe.Infrastructure.Data;
using v2.Core.DTOs;
using v2.Core.Interfaces;
using v2.Core.Models;

public class ParkingLotService : IParkingLots
{
    private readonly ApplicationDbContext _db;

    public ParkingLotService(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Create a new parking lot. Duplicate policy: same Name + Address = Conflict (409).
    /// (server.py also restricts this to authenticated users; enforce role/admin in controller/middleware.)
    /// </summary>
    public async Task<(int statusCode, object message)> CreateParkingLotAsync(ParkingLotCreateRequest dto)
    {
        try
        {
            var duplicate = await _db.ParkingLots
                .FirstOrDefaultAsync(p => p.Name == dto.Name && p.Address == dto.Address);

            if (duplicate != null)
            {
                return (409, new
                {
                    error = "Parking lot already exists",
                    data = new { duplicate.ID, duplicate.Name, duplicate.Address, duplicate.Location }
                });
            }

            var lot = new ParkingLot
            {
                ID = Guid.NewGuid(),
                Name = dto.Name,
                Location = dto.Location,
                Address = dto.Address,
                Capacity = dto.Capacity,
                Reserved = 0,
                Tariff = dto.Tariff,
                DayTariff = dto.DayTariff,
                CreatedAt = DateTime.UtcNow,
                latitude = dto.Latitude,
                longitude = dto.Longitude,
                Reservations = new List<Reservation>(),
                Sessions = new List<Session>()
            };

            _db.ParkingLots.Add(lot);
            await _db.SaveChangesAsync();

            return (201, new
            {
                status = "Success",
                parkingLot = new
                {
                    id = lot.ID,
                    lot.Name,
                    lot.Location,
                    lot.Address,
                    lot.Capacity,
                    lot.Reserved,
                    lot.Tariff,
                    lot.DayTariff,
                    lot.latitude,
                    lot.longitude,
                    lot.CreatedAt
                }
            });
        }
        catch
        {
            return (500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Patch-like update. Enforces duplicate policy on (Name + Address).
    /// </summary>
    public async Task<(int statusCode, object message)> UpdateParkingLotAsync(Guid id, ParkingLotCreateRequest dto)
    {
        try
        {
            var lot = await _db.ParkingLots.FirstOrDefaultAsync(p => p.ID == id);
            if (lot == null)
                return (404, new { error = "Parking lot not found" });

            var newName = string.IsNullOrWhiteSpace(dto.Name) ? lot.Name : dto.Name;
            var newAddress = string.IsNullOrWhiteSpace(dto.Address) ? lot.Address : dto.Address;

            var duplicate = await _db.ParkingLots
                .FirstOrDefaultAsync(p => p.ID != id && p.Name == newName && p.Address == newAddress);

            if (duplicate != null)
            {
                return (409, new
                {
                    error = "Parking lot with same name and address already exists",
                    data = new { duplicate.ID, duplicate.Name, duplicate.Address }
                });
            }

            if (!string.IsNullOrWhiteSpace(dto.Name)) lot.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Location)) lot.Location = dto.Location;
            if (!string.IsNullOrWhiteSpace(dto.Address)) lot.Address = dto.Address;
            if (dto.Capacity != 0) lot.Capacity = dto.Capacity;
            if (dto.Tariff != 0) lot.Tariff = dto.Tariff;
            if (dto.DayTariff != 0) lot.DayTariff = dto.DayTariff;
            if (dto.Latitude != 0) lot.latitude = dto.Latitude;
            if (dto.Longitude != 0) lot.longitude = dto.Longitude;

            _db.ParkingLots.Update(lot);
            await _db.SaveChangesAsync();

            return (200, new
            {
                status = "Success",
                parkingLot = new
                {
                    id = lot.ID,
                    lot.Name,
                    lot.Location,
                    lot.Address,
                    lot.Capacity,
                    lot.Reserved,
                    lot.Tariff,
                    lot.DayTariff,
                    lot.latitude,
                    lot.longitude,
                    lot.CreatedAt
                }
            });
        }
        catch
        {
            return (500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Delete parking lot.
    /// </summary>
    public async Task<(int statusCode, object message)> DeleteParkingLotAsync(Guid id)
    {
        try
        {
            var lot = await _db.ParkingLots.FirstOrDefaultAsync(p => p.ID == id);
            if (lot == null)
                return (404, new { error = "Parking lot not found" });

            _db.ParkingLots.Remove(lot);
            await _db.SaveChangesAsync();

            return (200, new { status = "Success", message = "Parking lot deleted" });
        }
        catch
        {
            return (500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get single parking lot by ID.
    /// </summary>
    public async Task<(int statusCode, object message)> GetParkingLotAsync(Guid id)
    {
        try
        {
            var lot = await _db.ParkingLots.FirstOrDefaultAsync(p => p.ID == id);
            if (lot == null)
                return (404, new { error = "Parking lot not found" });

            return (200, new
            {
                status = "Success",
                parkingLot = new
                {
                    id = lot.ID,
                    lot.Name,
                    lot.Location,
                    lot.Address,
                    lot.Capacity,
                    lot.Reserved,
                    lot.Tariff,
                    lot.DayTariff,
                    lot.latitude,
                    lot.longitude,
                    lot.CreatedAt
                }
            });
        }
        catch
        {
            return (500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get all parking lots.
    /// </summary>
    public async Task<(int statusCode, object message)> GetAllParkingLotsAsync()
    {
        try
        {
            var lots = await _db.ParkingLots
                .Select(lot => new
                {
                    id = lot.ID,
                    lot.Name,
                    lot.Location,
                    lot.Address,
                    lot.Capacity,
                    lot.Reserved,
                    lot.Tariff,
                    lot.DayTariff,
                    lot.latitude,
                    lot.longitude,
                    lot.CreatedAt
                })
                .ToListAsync();

            return (200, new { status = "Success", parkingLots = lots });
        }
        catch
        {
            return (500, new { error = "An unexpected error occurred." });
        }
    }

    // ==== Session endpoints (stubs) ====
    // server.py exposes /parking-lots/{id}/sessions/start and /stop, with pricing/payment checks.
    // Implement once your Session model + calculator are ready, then call from controller.

    public async Task<(int statusCode, object message)> StartSessionAsync(Guid parkingLotId, string licensePlate, Guid userId)
    {
        // TODO: load lot, check capacity, create Session { StartTime, UserId, LicensePlate, ParkingLotId }, save
        // TODO: mirror old pricing rules (if any) when starting.
        await Task.CompletedTask;
        return (501, new { error = "Not implemented yet: StartSessionAsync" });
    }

    public async Task<(int statusCode, object message)> StopSessionAsync(Guid parkingLotId, string licensePlate, Guid userId)
    {
        // TODO: find active session by lot + licensePlate + user, set EndTime
        // TODO: compute price (mirror server.py session_calculator), persist a Payment/Transaction if applicable
        await Task.CompletedTask;
        return (501, new { error = "Not implemented yet: StopSessionAsync" });
    }
}
