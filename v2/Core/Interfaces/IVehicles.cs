using v2.Core.Models;
using v2.Core.DTOs;

namespace v2.Core.Interfaces;
public interface IVehicles
{
    Task<Vehicle> CreateVehicleAsync(CreateVehicleDto vehicle);

    Task<Vehicle?> UpdateVehicleAsync(string licensePlate, CreateVehicleDto updatedVehicle);

    Task<bool> DeleteVehicleAsync(string licensePlate);

    Task<IEnumerable<Vehicle>> GetAllVehiclesAsync(Guid? userId = null);

    Task<IEnumerable<Reservation>> GetReservationsByVehicleAsync(string licensePlate);

    Task<VehicleHistoryDTO?> GetVehicleHistoryAsync(string licensePlate);
}