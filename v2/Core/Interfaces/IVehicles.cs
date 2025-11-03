using v2.Core.Models;
using v2.Core.DTOs;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;

namespace v2.core.Interfaces;
public interface IVehicles
{
    Task<(int statusCode, object message)> CreateVehicleAsync(CreateVehicleDto vehicle, string identityUserId);

    Task<(int statusCode, object message)> UpdateVehicleAsync(string licensePlate, UpdateVehicleDto updatedVehicle, string identityUserId);

    Task<(int statusCode, object message)> DeleteVehicleAsync(string lid, string identityUserId);

    Task<(IEnumerable<Vehicle> data, int statusCode, object message)> GetAllVehiclesAsync(string identityUserId);

    Task<(IEnumerable<Vehicle> data, int statusCode, object message)> GetAllVehiclesForUserAsync(string username, string identityUserId);

    Task<(IEnumerable<Reservation> data, int statusCode, object message)> GetReservationsByVehicleAsync(string vid, string identityUserId);

    Task<(IEnumerable<Session> data, int statusCode, object message)> GetVehicleHistoryAsync(string licensePlate, string identityUserId);
}