namespace v2.infrastructure.Services;

using v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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


    public async Task<(int statusCode, object message)> StartSessionAsync(Guid parkingLotId, string licensePlate, Guid userId)
    {
        try
        {
            
            var lot = await _db.ParkingLots.FirstOrDefaultAsync(p => p.ID == parkingLotId);
            if (lot == null)
            {
                return (404, new { error = "Parking lot not found" });
            }

          
            var activeSessionsCount = await _db.Sessions.CountAsync(s => s.ParkingLotID == parkingLotId && s.EndTime == null);
            if (activeSessionsCount >= lot.Capacity)
            {
                return (409, new { error = "Parking lot is full" });
            }

      
            var existingActiveSession = await _db.Sessions.FirstOrDefaultAsync(s =>
                s.ParkingLotID == parkingLotId &&
                s.LicensePlate == licensePlate &&
                s.EndTime == null);

            if (existingActiveSession != null)
            {
                return (409, new { error = "An active session for this license plate already exists in this parking lot." });
            }


            var newSession = new Session
            {
                ID = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                EndTime = null,
                LicensePlate = licensePlate,
                ParkingLotID = parkingLotId,
                ParkingLot = lot,
                // UserID = userId,
                PaymentStatus = PaymentStatus.Unpaid
            };
            
             if (userId != Guid.Empty)
            {
                newSession.UserID = userId;
            }

      
            _db.Sessions.Add(newSession);
            await _db.SaveChangesAsync();

       
            return (201, new
            {
                status = "Success",
                message = "Session started",
                session = new
                {
                    id = newSession.ID,
                    licensePlate = newSession.LicensePlate,
                    startTime = newSession.StartTime,
                    endTime = newSession.EndTime,
                    parkingLotId = newSession.ParkingLotID,
                    paymentStatus = newSession.PaymentStatus
                }
            });
        }
       catch (Exception ex)
    {
        return (500, new
        {
            error = "An unexpected error occurred while starting the session.",
            detail = ex.Message,
            inner = ex.InnerException?.Message,
            inner2 = ex.InnerException?.InnerException?.Message
        });
    }

    }

    public async Task<(int statusCode, object message)> StopSessionAsync(Guid parkingLotId, string licensePlate, Guid userId)
{
    try
    {
        
        var query = _db.Sessions
            .Include(s => s.ParkingLot)
            .Where(s =>
                s.ParkingLotID == parkingLotId &&
                s.LicensePlate == licensePlate &&
                s.EndTime == null);

       
        if (userId != Guid.Empty)
        {
            query = query.Where(s => s.UserID == userId);
        }

        var activeSession = await query.FirstOrDefaultAsync();

        if (activeSession == null)
        {
            return (404, new { error = "No active session found for this license plate and user in this parking lot." });
        }

       
        activeSession.EndTime = DateTime.UtcNow;

        
        var duration = activeSession.EndTime.Value - activeSession.StartTime;
        double totalHours = duration.TotalHours;

        float cost = (float)totalHours * activeSession.ParkingLot.Tariff;

        
        if (totalHours > (activeSession.ParkingLot.DayTariff / activeSession.ParkingLot.Tariff))
        {
            cost = activeSession.ParkingLot.DayTariff;
        }

        _db.Sessions.Update(activeSession);
        await _db.SaveChangesAsync();

        
        return (200, new
        {
            status = "Success",
            message = "Session stopped",
            session = new
            {
                id = activeSession.ID,
                licensePlate = activeSession.LicensePlate,
                startTime = activeSession.StartTime,
                endTime = activeSession.EndTime,
                parkingLotId = activeSession.ParkingLotID,
                paymentStatus = activeSession.PaymentStatus,
                price = activeSession.Price,
            },
            billing = new
            {
                durationInMinutes = duration.TotalMinutes,
                calculatedCost = cost
            }
        });
    }
    catch (Exception)
    {
        return (500, new { error = "An unexpected error occurred while stopping the session." });
    }
}


    public async Task<(int statusCode, object message)> GetAllSessionsForLotAsync(Guid parkingLotId)
    {
        try
        {
          
            var lotExists = await _db.ParkingLots.AnyAsync(p => p.ID == parkingLotId);
            if (!lotExists)
            {
                return (404, new { error = "Parking lot not found" });
            }

            var sessions = await _db.Sessions
                .Where(s => s.ParkingLotID == parkingLotId)
                .OrderByDescending(s => s.StartTime) 
                .ToListAsync();

            return (200, new { status = "Success", sessions });
        }
        catch (Exception ex)
        {
           
            return (500, new { error = "An unexpected error occurred." });
        }
    }

 
    public async Task<(int statusCode, object message)> GetSessionByIdAsync(Guid parkingLotId, Guid sessionId)
    {
        try
        {
            var session = await _db.Sessions
                .FirstOrDefaultAsync(s => s.ParkingLotID == parkingLotId && s.ID == sessionId);

            if (session == null)
            {
                return (404, new { error = "Session not found in this parking lot" });
            }

            return (200, new { status = "Success", session });
        }
        catch (Exception ex)
        {
           
            return (500, new { error = "An unexpected error occurred." });
        }
    }

    public async Task<(int statusCode, object message)> DeleteSessionAsync(Guid parkingLotId, Guid sessionId)
    {
        try
        {
            var session = await _db.Sessions
                .FirstOrDefaultAsync(s => s.ParkingLotID == parkingLotId && s.ID == sessionId);

            if (session == null)
            {
                return (404, new { error = "Session not found in this parking lot" });
            }

          

            _db.Sessions.Remove(session);
            await _db.SaveChangesAsync();

            return (200, new { status = "Success", message = "Session deleted" });
        }
        catch (Exception ex)
        {
     
            return (500, new { error = "An unexpected error occurred." });
        }
    }

}
