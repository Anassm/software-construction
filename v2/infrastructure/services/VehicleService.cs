using Microsoft.EntityFrameworkCore;
using v2.core.Interfaces;
using v2.Core.Models;
using v2.Infrastructure.Data;
using v2.Core.DTOs;

namespace v2.infrastructure.Services;

public class VehicleService : IVehicles
{
    private readonly ApplicationDbContext _dbContext;

    public VehicleService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(int statusCode, object message)> CreateVehicleAsync(CreateVehicleDto dto, string identityUserId)
    {
        try
        {
            var user = await _dbContext.Users.Where(u => u.IdentityUserId == identityUserId).FirstOrDefaultAsync();
            if (user == null)
                return (404, new { error = "User not found" });

            string licensePlate = dto.LicensePlate.Replace("-", "");
            var duplicatevehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == licensePlate && v.UserID == user.ID);
            if (duplicatevehicle != null)
                return (409, new { error = "Vehicle already exists", data = duplicatevehicle });


            var vehicle = new Vehicle
            {
                LicensePlate = licensePlate,
                Make = dto.Make,
                Model = dto.Model,
                Color = dto.Color,
                Year = dto.Year,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                UserID = user.ID,
                Reservations = new List<Reservation>()
            };

            _dbContext.Vehicles.Add(vehicle);
            await _dbContext.SaveChangesAsync();
            return (201, new { status = "Success", vehicle });
        }
        catch
        {
            return (500, new { error = "An unexpected error occurred." });
        }
    }

    public async Task<(int statusCode, object message)> UpdateVehicleAsync(string lid, UpdateVehicleDto updatedVehicle, string identityUserId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (user == null)
                return (404, new { error = "User not found" });

            var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.ID.ToString() == lid && v.UserID == user.ID || v.LicensePlate.Replace("-", "") == lid.Replace("-", "") && v.UserID == user.ID || v.OldID == lid && v.UserID == user.ID);
            if (vehicle == null)
                return (404, new { error = "Vehicle not found" });

            var duplicatevehicle = _dbContext.Vehicles.FirstOrDefault(v => v.LicensePlate == updatedVehicle.LicensePlate && v.UserID == user.ID);
            if (duplicatevehicle != null)
                return (409, new { error = "Vehicle already exists", vehicle = duplicatevehicle });

            if (updatedVehicle.LicensePlate != "")
                vehicle.LicensePlate = updatedVehicle.LicensePlate;

            if (updatedVehicle.Make != "")
                vehicle.Make = updatedVehicle.Make;

            if (updatedVehicle.Model != "")
                vehicle.Model = updatedVehicle.Model;

            if (updatedVehicle.Color != "")
                vehicle.Color = updatedVehicle.Color;

            if (updatedVehicle.Year != 0)
                vehicle.Year = updatedVehicle.Year;

            if (updatedVehicle.UserId != Guid.Empty)
                vehicle.UserID = updatedVehicle.UserId;

            _dbContext.Vehicles.Update(vehicle);
            await _dbContext.SaveChangesAsync();
            return (200, new { status = "success", vehicle });
        }
        catch
        {
            return (500, new { error = "An unexpected error occurred." });
        }
    }

    public async Task<(int statusCode, object message)> DeleteVehicleAsync(string lid, string identityUserId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (user == null)
                return (404, new { error = "User not found" });

            var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.ID.ToString() == lid && v.UserID == user.ID || v.LicensePlate == lid && v.UserID == user.ID || v.OldID == lid && v.UserID == user.ID);
            if (vehicle == null)
                return (404, new { error = "Vehicle not found!" });

            _dbContext.Vehicles.Remove(vehicle);
            _dbContext.SaveChanges();

            return (200, new { status = "Deleted" });
        }
        catch
        {
            return (500, new { error = "An unexpected error occurred." });
        }
    }

    public async Task<(IEnumerable<Vehicle> data, int statusCode, object message)> GetAllVehiclesAsync(string identityUserId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (user == null)
                return (null!, 404, new { error = "User not found" });

            var vehicles = _dbContext.Vehicles.Where(v => v.UserID == user.ID);
            return (vehicles, 200, new { });
        }
        catch
        {
            return (null!, 500, new { error = "An unexpected error occurred." });
        }
    }

    public async Task<(IEnumerable<Vehicle> data, int statusCode, object message)> GetAllVehiclesForUserAsync(string? username, string identityUserId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (user == null)
                return (null!, 404, new { error = "User not found" });

            if (user.Role.ToLower() != "admin")
                return (null!, 401, new { Error = "Unauthorized" });

            User? lookupUser;
            List<Vehicle> vehicles;
            if (username != null || username != "")
            {
                lookupUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (lookupUser == null)
                    return (null!, 404, new { error = "User not found" });
                vehicles = await _dbContext.Vehicles.Where(v => v.UserID == lookupUser.ID).ToListAsync();
            }
            else
            {
                vehicles = await _dbContext.Vehicles.ToListAsync();
            }
            return (vehicles, 200, null!);
        }
        catch
        {
            return (null!, 500, new { error = "An unexpected error occurred." });
        }
    }

    public async Task<(IEnumerable<Reservation> data, int statusCode, object message)> GetReservationsByVehicleAsync(string vid, string identityUserId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (user == null)
                return (null!, 404, new { error = "User not found" });

            var vehicle = _dbContext.Vehicles.Where(v => v.ID.ToString().ToLower() == vid && v.UserID == user.ID || v.OldID == vid && v.UserID == user.ID).AsQueryable();
            if (!vehicle.Any())
                return (null!, 404, new { error = "Vehicle not found" });

            var reservations = await vehicle.SelectMany(v => v.Reservations).ToListAsync();
            return (reservations, 200, null!);
        }
        catch
        {
            return (null!, 500, new { error = "An unexpected error occurred." });
        }
    }

    public async Task<(IEnumerable<Session> data, int statusCode, object message)> GetVehicleHistoryAsync(string vid, string identityUserId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (user == null)
                return (null!, 404, new { error = "User not found" });

            var vehicle = await _dbContext.Vehicles.Where(v => v.ID.ToString().ToLower() == vid && v.UserID == user.ID).FirstOrDefaultAsync();
            if (vehicle == null)
                return (null!, 404, new { error = "Vehicle not found" });

            var history = await _dbContext.Sessions.Where(s => s.LicensePlate == vehicle.LicensePlate).ToListAsync();
            return (history, 200, null!);
        }
        catch
        {
            return (null!, 500, new { error = "An unexpected error occurred." });
        }
    }

    public async Task<(int statusCode, object message)> StartSessionByEntryAsync(string lid, string parkingLotId, string identityUserId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
            if (user == null)
                return (404, new { error = "User not found" });

            Guid lidGuidId;
            bool isGuid = Guid.TryParse(lid, out lidGuidId);

            Vehicle? vehicle;
            if (isGuid)
            {
                vehicle = await _dbContext.Vehicles
                    .FirstOrDefaultAsync(v => v.ID == lidGuidId);
            }
            else
            {
                vehicle = await _dbContext.Vehicles
                    .FirstOrDefaultAsync(v => v.OldID == lid);
            }
            if (vehicle == null)
                return (404, new { error = "Vehicle not found" });

            Guid parkingLotGuidId;
            bool isGuid2 = Guid.TryParse(parkingLotId, out parkingLotGuidId);

            ParkingLot? parkingLot;
            if (isGuid2)
            {
                parkingLot = await _dbContext.ParkingLots.FirstOrDefaultAsync(p => p.ID == lidGuidId);
            }
            else
            {
                parkingLot = await _dbContext.ParkingLots.FirstOrDefaultAsync(p => p.OldID == parkingLotId);
            }
            if (parkingLot == null)
                return (404, new { error = "Parkinglot not found" });

            Session session = new Session
            {
                LicensePlate = vehicle.LicensePlate,
                UserID = user.ID,
                ParkingLotID = parkingLot.ID,

            };
            _dbContext.Sessions.Add(session);
            await _dbContext.SaveChangesAsync();
            return (201, new { status = "Success", session } );
        }
        catch
        {
            return (500, new { error = "An unexpected error occurred." });
        }
    }
}
