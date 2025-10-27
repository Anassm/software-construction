namespace v2.core.Interfaces;

public interface IVehicles
{
    public Task<List<Vehicle>> GetAllVehiclesAsync();
    public Task<Vehicle?> GetVehicleByIdAsync(int id);
    public Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
    public Task<Vehicle?> UpdateVehicleAsync(int id, Vehicle vehicle);
    public Task<bool> DeleteVehicleAsync(int id);
}
