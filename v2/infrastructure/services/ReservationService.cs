using v2.Infrastructure.Data;
using v2.core.Interfaces;
using v2.Core.Models;
using v2.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace v2.Infrastructure.Services;

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

        var rawPlate = request.LicensePlate;
        var cleanedPlate = request.LicensePlate.Replace("-", "");

        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v =>
                v.LicensePlate == rawPlate || v.LicensePlate == cleanedPlate)
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

    public async Task<IEnumerable<Reservation>> GetReservationsForUserAsync(string identityUserId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId)
            ?? throw new ArgumentException("User not found.");

        var reservations = await _db.Reservations
            .Include(r => r.Vehicle)
            .Include(r => r.ParkingLot)
            .Where(r => r.UserID == user.ID)
            .ToListAsync();

        return reservations;
    }

    public async Task<bool> DeleteReservationForUserAsync(Guid reservationId, string identityUserId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId)
            ?? throw new ArgumentException("User not found.");

        var reservation = await _db.Reservations
            .FirstOrDefaultAsync(r => r.ID == reservationId && r.UserID == user.ID);

        if (reservation == null)
        {
            return false;
        }

        _db.Reservations.Remove(reservation);
        await _db.SaveChangesAsync();
        return true;
    }
}
