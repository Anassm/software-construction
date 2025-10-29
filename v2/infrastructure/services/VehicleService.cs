namespace v2.infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using v2.Core.Interfaces;
using v2.Core.Models;
using ChefServe.Infrastructure.Data;
using v2.Core.DTOs;

public class VehicleService : IVehicles
{
    private readonly ApplicationDbContext _dbContext;

    public VehicleService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Vehicle> CreateVehicleAsync(CreateVehicleDto dto)
    {
        if (dto == null)
            return null!;

        var vehicle = new Vehicle
        {
            LicensePlate = dto.LicensePlate,
            Make = dto.Make,
            Model = dto.Model,
            Color = dto.Color,
            Year = dto.Year,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UserID = dto.UserID,
            Reservations = new List<Reservation>()
        };

        _dbContext.Vehicles.Add(vehicle);
        await _dbContext.SaveChangesAsync();
        return vehicle;
    }

    public async Task<Vehicle?> UpdateVehicleAsync(string licensePlate, CreateVehicleDto updatedVehicle)
    {
        if (string.IsNullOrWhiteSpace(licensePlate) || updatedVehicle == null)
            return null;

        var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
        if (vehicle == null)
            return null;

        vehicle.Make = updatedVehicle.Make;
        vehicle.Model = updatedVehicle.Model;
        vehicle.Color = updatedVehicle.Color;
        vehicle.Year = updatedVehicle.Year;
        vehicle.UserID = updatedVehicle.UserID;

        _dbContext.Vehicles.Update(vehicle);
        await _dbContext.SaveChangesAsync();
        return vehicle;
    }

    public async Task<bool> DeleteVehicleAsync(string licensePlate)
    {
        var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
        if (vehicle == null)
            return false;

        _dbContext.Vehicles.Remove(vehicle);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync(Guid? userId = null)
    {
        if (userId == null)
        {
            return await _dbContext.Vehicles.ToListAsync();
        }
        return await _dbContext.Vehicles
            .Where(v => v.UserID == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByVehicleAsync(string licensePlate)
    {
        var vehicle = await _dbContext.Vehicles
            .Include(v => v.Reservations)
            .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

        return vehicle?.Reservations ?? Enumerable.Empty<Reservation>();
    }

    public async Task<VehicleHistoryDTO?> GetVehicleHistoryAsync(string licensePlate)
    {
        var vehicle = await _dbContext.Vehicles
            .Include(v => v.Reservations)
            .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

        if (vehicle == null)
            return null;

        var vehicleHistory = new VehicleHistoryDTO
        {
            Vehicle = vehicle,
            Reservations = vehicle.Reservations
        };

        return vehicleHistory;
    }
}
