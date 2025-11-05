using v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using v2.core.Interfaces;
using v2.Core.DTOs;
using v2.Core.Models;
using System.Collections.Generic;

namespace v2.infrastructure.Services;

public class ReservationService : IReservation
{
    private readonly ApplicationDbContext _db;
    public ReservationService(ApplicationDbContext db) => _db = db;

    public async Task<Reservation> CreateReservationAsync(ReservationCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LicensePlate))
            throw new ArgumentException("License plate is required.");

        if (request.EndDate <= request.StartDate)
            throw new ArgumentException("EndDate must be greater than StartDate.");

        var lot = await _db.ParkingLots.FindAsync(request.ParkingLotId)
            ?? throw new ArgumentException("Parking lot not found.");

        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.LicensePlate == request.LicensePlate)
            ?? throw new ArgumentException("Vehicle with given license plate not found.");

        var reservation = new Reservation
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Pending",
            TotalPrice = 0f,
            UserID = vehicle.UserID,
            ParkingLotID = lot.ID,
            VehicleID = vehicle.ID,
            CreatedAt = DateTime.UtcNow
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        return reservation;
    }

    public async Task<IEnumerable<Reservation>> GetReservationsAsync(Guid userId)
    {
        var reservations = await _db.Reservations
            .Where(r => r.UserID == userId)
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingLot)
            .OrderBy(r => r.StartDate)
            .ToListAsync();
            
        return reservations;
    }

}