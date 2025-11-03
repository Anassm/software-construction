using ChefServe.Infrastructure.Data;         // ApplicationDbContext
using Microsoft.EntityFrameworkCore;         // FirstOrDefaultAsync, Include
using v2.core.Interfaces;
using v2.Core.DTOs;
using v2.Core.Models;

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

        var lot = await _db.ParkingLots
            .FirstOrDefaultAsync(p => p.ID == request.ParkingLotId);
        if (lot is null)
            throw new ArgumentException("Parking lot not found.");

        var vehicle = await _db.Vehicles
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.LicensePlate == request.LicensePlate);
        if (vehicle is null)
            throw new ArgumentException("Vehicle with given license plate not found.");

        var reservation = new Reservation
        {
            ID = Guid.NewGuid(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 0f,
            UserID = vehicle.UserID,
            ParkingLotID = lot.ID,
            VehicleID = vehicle.ID
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        return reservation;
    }
}
