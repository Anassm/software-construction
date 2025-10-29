using v2.Core.Interfaces;
using v2.Core.Models;
using v2.Core.DTOs;
using ChefServe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace v2.Infrastructure.Services;

public class VehicleService : IVehicles
{
    private readonly ApplicationDbContext _context;

    public VehicleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Vehicle> CreateVehicleAsync(CreateVehicleDto vehicle)
    {
        // Check if vehicle with this license plate already exists
        var existingVehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.LicensePlate == vehicle.LicensePlate);

        if (existingVehicle != null)
        {
            throw new InvalidOperationException($"Vehicle with license plate {vehicle.LicensePlate} already exists.");
        }

        var newVehicle = new Vehicle
        {
            LicensePlate = vehicle.LicensePlate,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Color = vehicle.Color,
            Year = vehicle.Year,
            UserID = vehicle.UserID,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _context.Vehicles.AddAsync(newVehicle);
        await _context.SaveChangesAsync();

        return newVehicle;
    }

    public async Task<Vehicle?> UpdateVehicleAsync(string licensePlate, CreateVehicleDto updatedVehicle)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

        if (vehicle == null)
        {
            return null;
        }

        // Update the vehicle properties
        vehicle.Make = updatedVehicle.Make;
        vehicle.Model = updatedVehicle.Model;
        vehicle.Color = updatedVehicle.Color;
        vehicle.Year = updatedVehicle.Year;

        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();

        return vehicle;
    }

    public async Task<bool> DeleteVehicleAsync(string licensePlate)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

        if (vehicle == null)
        {
            return false;
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync(Guid? userId = null)
    {
        var query = _context.Vehicles.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(v => v.UserID == userId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByVehicleAsync(string licensePlate)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Reservations)
            .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

        if (vehicle == null)
        {
            return Enumerable.Empty<Reservation>();
        }

        return vehicle.Reservations;
    }

    public async Task<VehicleHistoryDTO?> GetVehicleHistoryAsync(string licensePlate)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Reservations)
            .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

        if (vehicle == null)
        {
            return null;
        }

        return new VehicleHistoryDTO
        {
            Vehicle = vehicle,
            Reservations = vehicle.Reservations
        };
    }
}
