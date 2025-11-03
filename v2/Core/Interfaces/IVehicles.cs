// using v2.Core.Models;
// using v2.Core.DTOs;
// using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;

// namespace v2.Core.Interfaces;
// public interface IVehicles
// {
//     Task<(int statusCode, object message)> CreateVehicleAsync(CreateVehicleDto vehicle, string identityUserId);

//     Task<(int statusCode, object message)> UpdateVehicleAsync(string licensePlate, UpdateVehicleDto updatedVehicle, string identityUserId);

//     Task<(bool success, int statusCode, object message)> DeleteVehicleAsync(string licensePlate);

//     Task<(IEnumerable<Vehicle> data, int statusCode, object message)> GetAllVehiclesAsync(Guid? userId = null);

//     Task<(IEnumerable<Reservation> data, int statusCode, object message)> GetReservationsByVehicleAsync(string licensePlate);

//     Task<(VehicleHistoryDTO? data, int statusCode, object message)> GetVehicleHistoryAsync(string licensePlate);
// }