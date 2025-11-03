// namespace v2.infrastructure.Services;

// using Microsoft.EntityFrameworkCore;
// using v2.Core.Interfaces;
// using v2.Core.Models;
// using ChefServe.Infrastructure.Data;
// using v2.Core.DTOs;

// public class VehicleService : IVehicles
// {
//     private readonly ApplicationDbContext _dbContext;

//     public VehicleService(ApplicationDbContext dbContext)
//     {
//         _dbContext = dbContext;
//     }

//     public async Task<(int statusCode, object message)> CreateVehicleAsync(CreateVehicleDto dto, string identityUserId)
//     {
//         try
//         {
//             var user = await _dbContext.Users.Where(u => u.IdentityUserId == identityUserId).FirstOrDefaultAsync();
//             if (user == null)
//                 return (404, new { error = "User not found" });

//             string licensePlate = dto.LicensePlate.Replace("-", "");
//             var duplicatevehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == licensePlate && v.UserID == user.ID);
//             if (duplicatevehicle != null)
//                 return (409, new { error = "Vehicle already exists", data = duplicatevehicle });


//             var vehicle = new Vehicle
//             {
//                 LicensePlate = dto.LicensePlate,
//                 Make = dto.Make,
//                 Model = dto.Model,
//                 Color = dto.Color,
//                 Year = dto.Year,
//                 CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
//                 UserID = user.ID,
//                 Reservations = new List<Reservation>()
//             };

//             _dbContext.Vehicles.Add(vehicle);
//             await _dbContext.SaveChangesAsync();
//             return (201, new { status = "Success", vehicle });
//         }
//         catch
//         {
//             return (500, new { error = "An unexpected error occurred." });
//         }
//     }

//     public async Task<(int statusCode, object message)> UpdateVehicleAsync(string lid, UpdateVehicleDto updatedVehicle, string identityUserId)
//     {
//         try
//         {
//             var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
//             if (user == null)
//                 return (404, new { error = "User not found" });

//             var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.ID.ToString() == lid && v.UserID == user.ID || v.LicensePlate == lid && v.UserID == user.ID );
//             if (vehicle == null)
//                 return (404, new { error = "Vehicle not found" });

//             var duplicatevehicle = _dbContext.Vehicles.FirstOrDefault(v => v.LicensePlate == updatedVehicle.LicensePlate && v.UserID == user.ID);
//             if (duplicatevehicle != null)
//                 return (409, new { error = "Vehicle already exists", vehicle = duplicatevehicle });
        
//             if (updatedVehicle.LicensePlate != "")
//                 vehicle.LicensePlate = updatedVehicle.LicensePlate;

//             if (updatedVehicle.Make != "")
//                 vehicle.Make = updatedVehicle.Make;

//             if (updatedVehicle.Model != "")
//                 vehicle.Model = updatedVehicle.Model;

//             if (updatedVehicle.Color != "")
//                 vehicle.Color = updatedVehicle.Color;

//             if (updatedVehicle.Year != 0)
//                 vehicle.Year = updatedVehicle.Year;

//             if (updatedVehicle.UserId != Guid.Empty)
//                 vehicle.UserID = updatedVehicle.UserId;
            
//             _dbContext.Vehicles.Update(vehicle);
//             await _dbContext.SaveChangesAsync();
//             return (200, new { status = "success", vehicle });
//         }
//         catch
//         {
//             return (500, new { error = "An unexpected error occurred." });
//         }
//     }

//     // Task<(bool success, int statusCode, object message)> DeleteVehicleAsync(string licensePlate)
//     // {
//     //     
//     // }

//     // Task<(IEnumerable<Vehicle> data, int statusCode, object message)> GetAllVehiclesAsync(Guid? userId = null)
//     // {
        
//     // }

//     // Task<(IEnumerable<Reservation> data, int statusCode, object message)> GetReservationsByVehicleAsync(string licensePlate)
//     // {
        
//     // }

//     // Task<(VehicleHistoryDTO? data, int statusCode, object message)> GetVehicleHistoryAsync(string licensePlate)
//     // {
        
//     // }
// }
