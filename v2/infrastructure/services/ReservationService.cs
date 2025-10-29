using ChefServe.Infrastructure.Data;         // ApplicationDbContext
using Microsoft.EntityFrameworkCore;         // FirstOrDefaultAsync, Include
using Microsoft.Extensions.Logging;
using v2.Core.Interfaces;
using v2.Core.DTOs;
using v2.Core.Models;

namespace v2.infrastructure.Services;

public class ReservationService : IReservation
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ReservationService> _logger;
    
    public ReservationService(ApplicationDbContext db, ILogger<ReservationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Reservation> CreateReservationAsync(ReservationCreateRequest request)
    {
        _logger.LogInformation("Creating reservation for vehicle {LicensePlate} at parking lot {ParkingLotId}", 
            request.LicensePlate, request.ParkingLotId);

        // Input validation
        if (string.IsNullOrWhiteSpace(request.LicensePlate))
            throw new ArgumentException("License plate is required.");
        
        if (request.EndDate <= request.StartDate)
            throw new ArgumentException("EndDate must be greater than StartDate.");

        // Validate dates are not in the past
        if (request.StartDate < DateTime.UtcNow)
            throw new ArgumentException("StartDate cannot be in the past.");

        // Validate maximum reservation duration (365 days)
        if (request.EndDate > request.StartDate.AddDays(365))
            throw new ArgumentException("Reservation cannot exceed 365 days.");

        // Validate minimum reservation duration (1 hour)
        var minDuration = TimeSpan.FromHours(1);
        if (request.EndDate - request.StartDate < minDuration)
            throw new ArgumentException("Minimum reservation duration is 1 hour.");

        var lot = await _db.ParkingLots
            .FirstOrDefaultAsync(p => p.ID == request.ParkingLotId);
        if (lot is null)
        {
            _logger.LogWarning("Parking lot {ParkingLotId} not found", request.ParkingLotId);
            throw new ArgumentException("Parking lot not found.");
        }

        var vehicle = await _db.Vehicles
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.LicensePlate == request.LicensePlate);
        if (vehicle is null)
        {
            _logger.LogWarning("Vehicle with license plate {LicensePlate} not found", request.LicensePlate);
            throw new ArgumentException("Vehicle with given license plate not found.");
        }

        // Check for overlapping reservations for the same vehicle
        var hasOverlappingReservations = await _db.Reservations
            .Where(r => r.VehicleID == vehicle.ID
                && r.Status != "Cancelled"
                && r.StartDate < request.EndDate 
                && r.EndDate > request.StartDate)
            .AnyAsync();

        if (hasOverlappingReservations)
        {
            _logger.LogWarning("Vehicle {LicensePlate} already has a reservation for the selected dates", request.LicensePlate);
            throw new ArgumentException("Vehicle already has a reservation for the selected dates.");
        }

        // Check parking lot capacity
        var existingReservations = await _db.Reservations
            .Where(r => r.ParkingLotID == request.ParkingLotId 
                && r.Status != "Cancelled"
                && r.StartDate < request.EndDate 
                && r.EndDate > request.StartDate)
            .CountAsync();

        if (existingReservations >= lot.Capacity)
        {
            _logger.LogWarning("Parking lot {ParkingLotId} is fully booked for the selected dates", request.ParkingLotId);
            throw new ArgumentException("Parking lot is fully booked for the selected dates.");
        }

        // Calculate total price
        var duration = (request.EndDate - request.StartDate).TotalDays;
        var totalPrice = (float)(duration * lot.DayTariff);

        // Use transaction to ensure data consistency
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var reservation = new Reservation
            {
                ID = Guid.NewGuid(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                TotalPrice = totalPrice,
                UserID = vehicle.UserID,
                ParkingLotID = lot.ID,
                VehicleID = vehicle.ID
            };

            _db.Reservations.Add(reservation);
            
            // Update parking lot reserved count
            lot.Reserved++;
            
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Successfully created reservation {ReservationId} for vehicle {LicensePlate}", 
                reservation.ID, request.LicensePlate);

            return reservation;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating reservation for vehicle {LicensePlate}", request.LicensePlate);
            throw;
        }
    }
}
