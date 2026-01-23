using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using v2.Infrastructure.Data;
using v2.Core.Models;
using System.Data.Common;
using System.Reflection.Metadata;

namespace v2.Infrastructure.Data
{
    public class ImportService
    {
        private readonly ApplicationDbContext _db;
        public ImportService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task ImportUsersAsync(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var users = JsonConvert.DeserializeObject<List<UserJson>>(json);

            foreach (var u in users)
            {
                var identity = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = u.username,
                    Email = u.email,
                    EmailConfirmed = true
                };

                var hasher = new PasswordHasher<IdentityUser>();
                identity.PasswordHash = hasher.HashPassword(identity, "TempPass123!");

                _db.Users.Add(new User
                {
                    ID = Guid.NewGuid(),
                    OldID = u.id,
                    IdentityUserId = identity.Id,
                    Username = u.username,
                    Name = u.name,
                    Email = u.email,
                    PhoneNumber = u.phone,
                    Role = u.role,
                    CreatedAt = DateTime.Parse(u.created_at),
                    BirthYear = u.birth_year,
                    IsActive = u.active,
                    IdentityUser = identity,
                    Vehicles = new List<Vehicle>(),
                    Sessions = new List<Session>(),
                    Reservations = new List<Reservation>()
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task ImportVehiclesAsync(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var vehicles = JsonConvert.DeserializeObject<List<VehicleJson>>(json);

            foreach (var v in vehicles)
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.OldID == v.user_id);
                if (user == null)
                {
                    Console.WriteLine($"Waarschuwing: Geen gebruiker gevonden voor vehicle {v.id} met user_id {v.user_id}. Overslaan.");
                    continue;
                }

                _db.Vehicles.Add(new Vehicle
                {
                    ID = Guid.NewGuid(),
                    OldID = v.id,
                    UserID = user.ID,
                    LicensePlate = v.license_plate,
                    Make = v.make,
                    Model = v.model,
                    Color = v.color,
                    Year = v.year,
                    CreatedAt = DateTime.Parse(v.created_at)
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task ImportParkingLotsAsync(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var lots = JsonConvert.DeserializeObject<Dictionary<string, ParkingLotJson>>(json);

            foreach (var p in lots.Values)
            {
                _db.ParkingLots.Add(new ParkingLot
                {
                    ID = Guid.NewGuid(),
                    OldID = p.id,
                    Name = p.name,
                    Location = p.location,
                    Address = p.address,
                    Capacity = p.capacity,
                    Reserved = p.reserved,
                    Tariff = (float)p.tariff,
                    DayTariff = (float)p.daytariff,
                    CreatedAt = DateTime.Parse(p.created_at),
                    latitude = (float)p.coordinates.lat,
                    longitude = (float)p.coordinates.lng,

                    Reservations = new List<Reservation>(),
                    Sessions = new List<Session>()
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task ImportReservationsAsync(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var reservations = JsonConvert.DeserializeObject<List<ReservationJson>>(json);

            foreach (var r in reservations)
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.OldID == r.user_id);
                if (user == null)
                {
                    Console.WriteLine($"Waarschuwing: Geen gebruiker gevonden voor reservation {r.id} met user_id {r.user_id}. Overslaan.");
                    continue;
                }

                var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.OldID == r.vehicle_id);
                if (vehicle == null)
                {
                    Console.WriteLine($"Waarschuwing: Geen voertuig gevonden voor reservation {r.id} met vehicle_id {r.vehicle_id}. Overslaan.");
                    continue;
                }

                var parkingLot = await _db.ParkingLots.FirstOrDefaultAsync(p => p.OldID == r.parking_lot_id);
                if (parkingLot == null)
                {
                    Console.WriteLine($"Waarschuwing: Geen parking lot gevonden voor reservation {r.id} met parking_lot_id {r.parking_lot_id}. Overslaan.");
                    continue;
                }

                _db.Reservations.Add(new Reservation
                {
                    ID = Guid.NewGuid(),
                    OldID = r.id,
                    UserID = user.ID,
                    ParkingLotID = parkingLot.ID,
                    VehicleID = vehicle.ID,
                    StartDate = DateTime.Parse(r.start_time),
                    EndDate = DateTime.Parse(r.end_time),
                    Status = r.status,
                    TotalPrice = (float)r.cost,
                    CreatedAt = DateTime.Parse(r.created_at)
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task ImportPaymentsAsync(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var payments = JsonConvert.DeserializeObject<List<PaymentJson>>(json);

            foreach (var p in payments)
            {
                _db.Payments.Add(new Payment
                {
                    ID = Guid.NewGuid(),
                    Amount = (decimal)p.amount,
                    Initiator = p.initiator,
                    CreatedAt = DateTime.Parse(p.created_at),
                    CompletedAt = DateTime.Parse(p.completed),
                    Hash = p.hash,
                    TransactionAmount = (decimal)p.t_data.amount,
                    TransactionDate = DateTime.Parse(p.t_data.date),
                    TransactionMethod = p.t_data.method,
                    TransactionIssuer = p.t_data.issuer,
                    TransactionBank = p.t_data.bank
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task ImportAllSessionsAsync(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, "*.json");
            const int batchSize = 5000; 

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var sessions = JsonConvert.DeserializeObject<Dictionary<string, SessionJson>>(json);

                var batch = new List<Session>();

                foreach (var s in sessions.Values)
                {
                    var parkingLot = await _db.ParkingLots.FirstOrDefaultAsync(p => p.OldID == s.parking_lot_id);
                    if (parkingLot == null)
                    {
                        Console.WriteLine($"Warning: ParkingLot {s.parking_lot_id} not found. Skipping session {s.id}.");
                        continue;
                    }

                    batch.Add(new Session
                    {
                        ID = Guid.NewGuid(),
                        OldID = s.id,
                        ParkingLotID = parkingLot.ID,
                        LicensePlate = s.licenseplate,
                        StartTime = DateTime.Parse(s.started),
                        EndTime = string.IsNullOrWhiteSpace(s.stopped) ? null : DateTime.Parse(s.stopped),
                        duration = s.duration_minutes,
                        Price = (float)s.cost,
                        PaymentStatus = s.payment_status.ToLower() == "paid"
                            ? PaymentStatus.Paid
                            : PaymentStatus.Unpaid
                    });

                    if (batch.Count >= batchSize)
                    {
                        _db.Sessions.AddRange(batch);
                        await _db.SaveChangesAsync();
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    _db.Sessions.AddRange(batch);
                    await _db.SaveChangesAsync();
                }
            }
        }
    }
}
